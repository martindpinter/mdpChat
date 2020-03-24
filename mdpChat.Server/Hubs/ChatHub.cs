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
        private readonly IDataManager _db;
        public ChatHub(IDataManager dataManager)
        {
            _db = dataManager;
        }

        #region Public, directly invokable methods (via SignalR connections)

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            _db.HandleConnection(Context.ConnectionId);
        }

        public async override Task OnDisconnectedAsync(Exception ex)    // if Exception is null, termination was intentional
        {
            _db.HandleDisconnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);

            // SignalR automatically removes disconnected ConnectionIds from SignalR Groups on Disconnect
        }

        public async Task OnLogin(string userName)
        {
            _db.HandleLogin(Context.ConnectionId, userName);
            await OnJoinGroup(userName, _globalChatRoomName);
        }

        public async Task OnCreateGroup(string groupName)
        {
            OperationResult res = _db.HandleCreateGroup(groupName);

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
            OperationResult res = _db.HandleDeleteGroup(groupName);

            if (res.Successful)
            {
                List<Client> clientsInGroup = _db.GetClientsInGroup(groupName);
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
            OperationResult res = _db.HandleJoinGroup(userName, groupName);
            
            if (res.Successful)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }

            Group group = _db.GetGroup(groupName);
            List<Message> msgList = _db.GetAllMessagesInGroup(group.Name);
            string serialized = JsonSerializer.Serialize(msgList);

            await Clients.Caller.SendAsync("ReceiveMessagesOfGroup", groupName, serialized);
            // await Clients.Group(groupName).SendAsync("ReceiveMessageUpdate", serialized);
            // await UpdateClients();
        }

        public async Task OnLeaveGroup(string userName, string groupName)
        {
            OperationResult res = _db.HandleLeaveGroup(userName, groupName);

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
            _db.HandleSendMessageToGroup(groupName, message, Context.ConnectionId);
            string authorName = _db.GetUserAttached(Context.ConnectionId).Name;
            await Clients.Group(groupName).SendAsync("ReceiveMessageFromGroup", authorName, groupName, message);
        }

        public async Task OnGetUsersInGroup(string groupName)
        {
            List<User> userList = _db.GetUsersInGroup(groupName);
            string serialized = JsonSerializer.Serialize(userList);
            await Clients.Caller.SendAsync("ReceiveUsersInGroup", groupName, serialized);
        }

        public async Task OnGetGroupsList()
        {
            List<Group> res = _db.GetAllGroups();
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












