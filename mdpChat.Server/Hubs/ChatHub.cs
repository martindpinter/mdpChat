using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;
using mdpChat.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        private readonly string _globalChatRoomName = "Global"; // move to config?
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IClientRepository _clientRepository;

        public ChatHub(IUserRepository userRepository,
                        IMessageRepository messageRepository,
                        IGroupRepository groupRepository,
                        IMembershipRepository membershipRepository,
                        IClientRepository clientRepository)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;           
            _groupRepository = groupRepository;
            _membershipRepository = membershipRepository;
            _clientRepository = clientRepository;
        }

        #region Public, directly invokable methods (via SignalR connections)

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            HandleConnection(Context.ConnectionId);

            // Global chat is considered a group
            await Groups.AddToGroupAsync(Context.ConnectionId, _globalChatRoomName); // TODO - add "Global" group name to config

            // UpdateClients(); // groups, message history, (changes) etc ....
        }

        public async override Task OnDisconnectedAsync(Exception ex)    // if Exception is null, termination was intentional
        {
            HandleDisconnection(Context.ConnectionId);
            
            // remove from all groups in which user is present... (todo)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _globalChatRoomName);
            await base.OnDisconnectedAsync(ex);
        }

        public async Task OnLogIn(string userName)
        {
            HandleLogIn(userName);

            await UpdateClients();
        }

        public async Task OnCreateGroup(string groupName)
        {
            throw new NotImplementedException();    // same as Join Group! (in signalr concept)
        }

        public async Task OnDeleteGroup(string groupName)
        {
            OperationResult res = HandleDeleteGroup(groupName);

            if (res.Successful)
            {
                List<Client> clientsInGroup = _clientRepository.GetClientsInGroup(groupName);
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
            OperationResult res = HandleJoinGroup(userName, groupName);
            
            if (res.Successful)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                // user's connectionId must be stored somewhere?
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }

            await UpdateClients();
        }

        public async Task OnLeaveGroup(string userName, string groupName)
        {
            OperationResult res = HandleLeaveGroup(userName, groupName);

            if (res.Successful)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }

            await UpdateClients();
            // update clients...
        }

        public async Task OnSendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(message);
        }
        #endregion

        #region Private, directly not invokable methods (via SignalR connections)

        private async Task UpdateClients()
        {
            // update all clients on changes to memberships/groups/etc
            throw new NotImplementedException();
        }

        private async Task ReturnErrorMessage(string errorMessage)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
        }
        #endregion

        #region DB syncing methods - THESE ARE NOT SUPPOSED TO BE IN THE HUB!

        public OperationResult HandleConnection(string connectionId)
        {
            // check if connectionId is already in db...?
            _clientRepository.Add(new Client() { ConnectionId = connectionId });
            return new OperationResult();
        }

        public OperationResult HandleLogIn(string userName)
        {
            // very primitive...
            if (!_userRepository.UserExists(userName))
            {
                _userRepository.Add(new User() { Name = userName });
            }

            HandleJoinGroup(userName, "General");
            return new OperationResult();
        }

        public OperationResult HandleJoinGroup(string userName, string groupName) 
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user != null && group != null)
            {
                if (!_groupRepository.IsFull(group))
                {
                    _membershipRepository.Add(new Membership()
                    {
                        UserId = user.Id,
                        GroupId = group.Id
                    });
                    return new OperationResult();
                }
                return new OperationResult() { ErrorMessage = $"{ group.Name } is full" };
            }
            return new OperationResult() { ErrorMessage = $"Can't find user({ user.Name }) or group({ group.Name }) to join" };
        }

        public OperationResult HandleLeaveGroup(string userName, string groupName)
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user != null && group != null)
            {
                Membership membership = _membershipRepository.GetMembership(user, group);
                if (membership != null)
                {
                    _membershipRepository.Add(new Membership()
                    {
                        UserId = user.Id,
                        GroupId = group.Id
                    });
                    return new OperationResult();
                }
                return new OperationResult() { ErrorMessage = $"No membership exists for user({ user.Name }) in group({ group.Name })."};
            }
            return new OperationResult() { ErrorMessage = $"Can't find user({ userName }) or group({ groupName }) to join in." };
        }

        public OperationResult HandleDeleteGroup(string groupName)
        {
            Group group = _groupRepository.GetGroup(groupName);
            if (group != null)
            {
                _membershipRepository.RemoveAllForGroup(group);
                return new OperationResult();
            }
            return new OperationResult() { ErrorMessage = $"Can't find group({ groupName} ) to delete" };
        }

        public OperationResult HandleSendMessageToGroup(string userName, string groupName, string message)
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user != null && group != null)
            {
                if (_membershipRepository.MembershipExists(user, group))
                {
                    _messageRepository.Add(new Message
                    {
                        AuthorId = user.Id,
                        GroupId = group.Id,
                        MessageBody = message,
                    });
                    return new OperationResult();
                }
                return new OperationResult() { ErrorMessage = $"User({ user.Name }) is not a member of group({ group.Name})."};
            }
            return new OperationResult() { ErrorMessage = $"Can't find user({ userName }) or group({ groupName }) to send message to."};

        }
        #endregion

    }
}












