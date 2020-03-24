using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;
using mdpChat.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        private readonly string _globalChatRoomName = "General"; // move to config?
        private readonly IDataManager DataStore;
        public ChatHub(IDataManager dataStore)
        {
            DataStore = dataStore;
        }

        #region Public, directly invokable methods (via SignalR connections)

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            DataStore.HandleConnection(Context.ConnectionId);
        }

        public async override Task OnDisconnectedAsync(Exception ex)    // if Exception is null, termination was intentional
        {
            DataStore.HandleDisconnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);

            // SignalR automatically removes disconnected ConnectionIds from SignalR Groups on Disconnect
        }

        public async Task OnLogin(string userName)
        {
            DataStore.HandleLogin(Context.ConnectionId, userName);

            await OnJoinGroup(userName, _globalChatRoomName);
        }

        public async Task OnCreateGroup(string groupName)
        {
            OperationResult res = DataStore.HandleCreateGroup(groupName);

            if (res.Successful)
            {
                // Same as OnJoinGroup, signalr concept-wise
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            else
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }
        }

        public async Task OnDeleteGroup(string groupName)
        {
            OperationResult res = DataStore.HandleDeleteGroup(groupName);

            if (res.Successful)
            {
                List<Client> clientsInGroup = DataStore.GetClientsInGroup(groupName);
                foreach (Client client in clientsInGroup)
                {
                    await Groups.RemoveFromGroupAsync(client.ConnectionId, groupName);
                }
            }
            else
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }
        }

        public async Task OnJoinGroup(string userName, string groupName) // none of these will be async, probably
        {
            OperationResult res = DataStore.HandleJoinGroup(userName, groupName);
            
            if (res.Successful)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }

            Group group = DataStore.GetGroup(groupName);
            List<Message> msgList = DataStore.GetAllMessagesInGroup(group.Name);
            string serialized = JsonSerializer.Serialize(msgList);

            await Clients.Caller.SendAsync("ReceiveMessagesOfGroup", groupName, serialized);
            // await Clients.Group(groupName).SendAsync("ReceiveMessageUpdate", serialized);
            // await UpdateClients();
        }

        public async Task OnLeaveGroup(string userName, string groupName)
        {
            OperationResult res = DataStore.HandleLeaveGroup(userName, groupName);

            if (res.Successful)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }
        }

        public async Task OnSendMessageToGroup(string groupName, string message)
        {
            DataStore.HandleSendMessageToGroup(groupName, message, Context.ConnectionId);
            string authorName = DataStore.GetUserAttached(Context.ConnectionId).Name;
            await Clients.Group(groupName).SendAsync("ReceiveMessageFromGroup", authorName, groupName, message);
        }

        public async Task OnGetUsersInGroup(string groupName)
        {
            List<User> userList = DataStore.GetUsersInGroup(groupName);
            string serialized = JsonSerializer.Serialize(userList);
            await Clients.Caller.SendAsync("ReceiveUsersInGroup", groupName, serialized);
        }

        public async Task OnGetGroupsList()
        {
            List<Group> res = DataStore.GetAllGroups();
            string serialized = JsonSerializer.Serialize(res);
            await Clients.Caller.SendAsync("ReceiveGroupsList", serialized);
        }

        #endregion

        #region Private, directly not invokable methods (via SignalR connections)

        private async Task ReturnErrorMessage(string errorMessage)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
        }
        #endregion


    }
}












