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
        private readonly string _globalChatRoomName = "General"; 
        private readonly IDataManager _db;
        public ChatHub(IDataManager dataManager)
        {
            _db = dataManager;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            _db.HandleConnection(Context.ConnectionId);
        }

        public async override Task OnDisconnectedAsync(Exception ex) 
        {
            User user = _db.GetUserAttached(Context.ConnectionId);
            _db.HandleDisconnection(Context.ConnectionId);

            if (user != null && !user.IsOnline)
            {
                await Clients.All.SendAsync("UserDisconnected", user.Name);
            }

            await base.OnDisconnectedAsync(ex);
        }

        public async Task OnLogin(string userName)
        {
            OperationResult res = _db.HandleLogin(Context.ConnectionId, userName);
            if (!res.Successful)
                return;

            List<string> groups = _db.GetAllGroups().Select(x => x.Name).ToList(); // db optimize!
            await Clients.Caller.SendAsync("LoginAccepted", userName, groups);
            await OnJoinGroup(userName, _globalChatRoomName);
        }

        public async Task OnCreateGroup(string groupName)
        {
            OperationResult res = _db.HandleCreateGroup(groupName);
            if (!res.Successful)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.All.SendAsync("GroupCreated", groupName);
        }

        public async Task OnDeleteGroup(string groupName)
        {
            OperationResult res = _db.HandleDeleteGroup(groupName);
            if (!res.Successful)
                return;

            List<Client> clientsInGroup = _db.GetClientsInGroup(groupName);
            foreach (Client client in clientsInGroup)
            {
                await Groups.RemoveFromGroupAsync(client.ConnectionId, groupName);
            }
            await Clients.All.SendAsync("GroupDeleted", groupName);
        }

        public async Task OnJoinGroup(string userName, string groupName)
        {
            OperationResult res = _db.HandleJoinGroup(userName, groupName);
            if (!res.Successful)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

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

        public async Task OnLeaveGroup(string groupName)
        {
            User user = _db.GetUserAttached(Context.ConnectionId);
            Group group = _db.GetGroup(groupName);
            if (user == null || group == null)
                return;

            OperationResult res = _db.HandleLeaveGroup(user, group); 

            if (!res.Successful)
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftChannel", group.Name, user.Name);
        }

        public async Task OnChangeGroup(string groupName)
        {
            User user = _db.GetUserAttached(Context.ConnectionId);
            Group group = _db.GetGroup(groupName);

            if (!_db.MembershipExists(user, group))
            {
                if (_db.IsGroupFull(group))
                    return;
                    
                _db.AddMembership(new Membership() 
                {
                    UserId = user.Id,
                    GroupId = group.Id
                });
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
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
    }
}