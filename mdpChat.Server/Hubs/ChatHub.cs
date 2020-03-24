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
        }

        public async override Task OnDisconnectedAsync(Exception ex)    // if Exception is null, termination was intentional
        {
            HandleDisconnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);

            // SignalR automatically removes disconnected ConnectionIds from SignalR Groups on Disconnect
        }

        public async Task OnLogIn(string userName)
        {
            HandleLogIn(userName);

            await OnJoinGroup(userName, _globalChatRoomName);
        }

        public async Task OnCreateGroup(string groupName)
        {
            OperationResult res = HandleCreateGroup(groupName);

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
            }
            else 
            {
                await ReturnErrorMessage(res.ErrorMessage);
            }

            Group group = _groupRepository.GetGroup(groupName);
            List<Message> msgList = _messageRepository.GetAllMessagesInGroup(group.Id);
            string serialized = JsonSerializer.Serialize(msgList);

            await Clients.Caller.SendAsync("ReceiveMessagesOfGroup", groupName, serialized);
            // await Clients.Group(groupName).SendAsync("ReceiveMessageUpdate", serialized);
            // await UpdateClients();
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
        }

        public async Task OnSendMessageToGroup(string groupName, string message)
        {
            HandleSendMessageToGroup(groupName, message);
            string authorName = _clientRepository.GetUserAttached(Context.ConnectionId).Name;
            await Clients.Group(groupName).SendAsync("ReceiveMessageFromGroup", authorName, groupName, message);
        }

        public async Task OnGetUsersInGroup(string groupName)
        {
            List<User> userList = _userRepository.GetUsersInGroup(groupName);
            string serialized = JsonSerializer.Serialize(userList);
            await Clients.Caller.SendAsync("ReceiveUsersInGroup", groupName, serialized);
        }

        public async Task OnGetGroupsList()
        {
            List<Group> res = _groupRepository.GetAllGroups();
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

        #region DB syncing methods - THESE ARE NOT SUPPOSED TO BE IN THE HUB!

        public OperationResult HandleConnection(string connectionId)
        {
            Client existingClient = _clientRepository.GetClient(connectionId);

            if (existingClient == null)
            {
                _clientRepository.Add(new Client() { ConnectionId = connectionId });
            }
            else
            {
                // make old connectionId vacant for new client
                _clientRepository.UpdateAsNew(existingClient);
            }
            return new OperationResult();
        }

        public OperationResult HandleDisconnection(string connectionId)
        {
            Client client = _clientRepository.GetClient(connectionId);
            
            if (client == null)
                return new OperationResult() { ErrorMessage = "Error disconnecting: no client exists with ConnectionId \"{ connectionId}\"" }; // not going to show ever, why do I even bother? exceptions pls...?

            _clientRepository.Remove(client);
            return new OperationResult();
        }

        public OperationResult HandleLogIn(string userName)
        {
            User user = _userRepository.GetUser(userName);

            if (user == null)
            {
                _userRepository.Add(new User() { Name = userName });
                user = _userRepository.GetUser(userName);  // not nice :(
            }

            _clientRepository.AssignUser(Context.ConnectionId, user);
            OperationResult res = HandleJoinGroup(userName, _globalChatRoomName);
            if (!res.Successful)
                return res;

            return new OperationResult();
        }

        public OperationResult HandleJoinGroup(string userName, string groupName) 
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user == null)
                return new OperationResult() { ErrorMessage = "Error joining group: no user exists with name \"{ userName }\"" };

            if (group == null) 
            {
                _groupRepository.Add(new Group() { Name = groupName });
                group = _groupRepository.GetGroup(groupName); // not nice, again :(  (return Group?)
            }

            if (_groupRepository.IsFull(group)) 
                return new OperationResult() { ErrorMessage = "Error joining group: group \"{ group.Name }\" is full" };

            _membershipRepository.Add(new Membership()
            {
                UserId = user.Id,
                GroupId = group.Id
            });
            return new OperationResult();
        }

        public OperationResult HandleLeaveGroup(string userName, string groupName)
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);
            Membership membership = _membershipRepository.GetMembership(user, group);

            if (user == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: no user exists with name \"{ userName }\"" };

            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: no group exists with name \"{ groupName }\"" };

            if (membership == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: user \"{ user.Name }\" is not a member of group \"{ group.Name }\"" };

            _membershipRepository.Remove(new Membership()
            {
                UserId = user.Id,
                GroupId = group.Id
            });
            return new OperationResult();
        }

        public OperationResult HandleCreateGroup(string groupName)
        {
            // TODO!!!!
            _groupRepository.Add(new Group()
            {
                Name = groupName
            });
            return new OperationResult();
        }

        public OperationResult HandleDeleteGroup(string groupName)
        {
            Group group = _groupRepository.GetGroup(groupName);
            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error deleting group: no group exists with name \"{ groupName } \"" };

            _membershipRepository.RemoveAllForGroup(group);
            return new OperationResult();
        }

        public OperationResult HandleSendMessageToGroup(string groupName, string message)
        {
            User user = _clientRepository.GetUserAttached(Context.ConnectionId);
            Group group = _groupRepository.GetGroup(groupName);

            if (user == null)
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ groupName }\": no user is bound to current connection" };

            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ groupName }\": no such group exists" };

            if (!_membershipRepository.MembershipExists(user, group))
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ group.Name }\": User \"{ user.Name }\" is not a member of the group" };

            _messageRepository.Add(new Message
            {
                AuthorId = user.Id,
                GroupId = group.Id,
                MessageBody = message,
            });
            return new OperationResult();
        }
        #endregion

    }
}












