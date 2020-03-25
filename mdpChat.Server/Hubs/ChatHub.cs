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
            User user = _db.GetUserAttached(Context.ConnectionId);

            _db.HandleDisconnection(Context.ConnectionId);

            if (user != null && !user.IsOnline)
            {
                await Clients.All.SendAsync("UserDisconnected", user.Name);
            }

            await base.OnDisconnectedAsync(ex);

            // SignalR automatically removes disconnected ConnectionIds from SignalR Groups on Disconnect
        }

        public async Task OnLogin(string userName)
        {
            _db.HandleLogin(Context.ConnectionId, userName);

            List<string> groups = _db.GetAllGroups().Select(x => x.Name).ToList(); // db optimize!
            await Clients.Caller.SendAsync("LoginAccepted", userName, groups);
            await OnJoinGroup(userName, _globalChatRoomName);
        }

        public async Task OnCreateGroup(string groupName)
        {
            OperationResult res = _db.HandleCreateGroup(groupName);

            if (res.Successful)
            {
                // Same as OnJoinGroup, signalr concept-wise
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.All.SendAsync("GroupCreated", groupName);
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
                await Clients.All.SendAsync("GroupDeleted", groupName);
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
            List<ApiMessage> apiMsgList = msgList.Select(x => new ApiMessage()
            {
                AuthorName = _db.GetUser(x.AuthorId).Name,
                GroupName = _db.GetGroup(x.GroupId).Name,
                MessageBody = x.MessageBody
            }).ToList();
            List<User> userList = _db.GetUsersInGroup(group.Name);
            List<string> userNames = userList.Select(x => x.Name).ToList(); // CLEAN AND WRITE SPEC DB QUERY

            await Clients.Caller.SendAsync("GroupJoined", groupName, userNames, apiMsgList);
            await Clients.Group(groupName).SendAsync("UserJoinedChannel", groupName, userName);
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

            await Clients.Group(groupName).SendAsync("UserLeftChannel", groupName, userName);
            // Update clients (refactor!)
            // List<User> usersInGroup = _db.GetUsersInGroup(groupName);
            // await Clients.Group(groupName).SendAsync("ReceiveUsersInGroup", groupName, JsonSerializer.Serialize(usersInGroup));
        }

        public async Task OnChangeGroup(string groupName)
        {
            User user = _db.GetUserAttached(Context.ConnectionId);
            Group group = _db.GetGroup(groupName);
            if (!_db.MembershipExists(user, group))
            {
                _db.AddMembership(new Membership() 
                {
                    UserId = user.Id,
                    GroupId = group.Id
                });
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                // return; // valami
            }

            List<Message> msgList = _db.GetAllMessagesInGroup(groupName);
            List<ApiMessage> apiMsgList = msgList.Select(x => new ApiMessage()
            {
                AuthorName = _db.GetUser(x.AuthorId).Name,
                GroupName = _db.GetGroup(x.GroupId).Name,
                MessageBody = x.MessageBody
            }).ToList();
            List<User> userList = _db.GetUsersInGroup(groupName);
            List<string> userNames = userList.Select(x => x.Name).ToList(); // CLEAN AND WRITE SPEC DB QUERY

            await Clients.Caller.SendAsync("GroupChangeApproved", groupName, apiMsgList, userNames);
            await Clients.Group(groupName).SendAsync("UserJoinedChannel", groupName, user.Name);
        }

        public async Task OnSendMessageToGroup(string groupName, string message)
        {
            OperationResult res = _db.HandleSendMessageToGroup(groupName, message, Context.ConnectionId);
            
            if (!res.Successful)
                return;

            string authorName = _db.GetUserAttached(Context.ConnectionId).Name;

            ApiMessage apiMsg = new ApiMessage()
            {
                AuthorName = _db.GetUserAttached(Context.ConnectionId).Name,
                GroupName = groupName,
                MessageBody = message
            };

            await Clients.Group(groupName).SendAsync("MessageReceived", groupName, apiMsg);
        }

        public async Task OnGetUsersInGroup(string groupName)
        {
            List<User> userList = _db.GetUsersInGroup(groupName);
            string serialized = JsonSerializer.Serialize(userList);
            await Clients.Caller.SendAsync("ReceiveUsersInGroup", groupName, serialized);
        }

        // public async Task OnGetGroupsList()
        // {
        //     List<Group> res = _db.GetAllGroups();
        //     string serialized = JsonSerializer.Serialize(res);
        //     await Clients.Caller.SendAsync("ReceiveGroupsList", serialized);
        // }
        #endregion

        #region Private, directly not invokable methods (via SignalR connections)
        private async Task ReturnErrorMessage(string errorMessage)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
        }
        #endregion


    }
}












