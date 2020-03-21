using System;
using System.Threading.Tasks;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;
using mdpChat.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IMembershipRepository _membershipRepository;

        #region Public, directly invokable methods (via SignalR connections)
        public ChatHub(IUserRepository userRepository,
                        IMessageRepository messageRepository,
                        IGroupRepository groupRepository,
                        IMembershipRepository membershipRepository)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;           
            _groupRepository = groupRepository;
            _membershipRepository = membershipRepository;
        }

        public async override Task OnConnectedAsync()
        {
            // Global chat is considered a group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Global");

            // update clients...

            // notify users in group (factor)
            string message = Context.ConnectionId + " joined the chat.";
            await Clients.All.SendAsync("ReceiveMessage", message);

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception ex)    // if Exception is null, termination was intentional
        {
            // remove from all groups in which user is present... (todo)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Global");
            await base.OnDisconnectedAsync(ex);
        }

        public async Task OnLogIn(string userName)
        {
            HandleLogIn(userName);

            await UpdateClients();
        }

        public async Task OnCreateGroup(string groupName)
        {
            throw new NotImplementedException();    // same as Join Group! (in signalr)
        }

        public async Task OnRemoveGroup(string groupName)
        {
            throw new NotImplementedException();

            // remove all connections from group!
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

            // TODO: move to HandleJoinGroup private function
            // ^with custom return type of possible error enums etc

            // !! valamit itt elfelejtek hogy a Group-muveleteknel (meg a tobbinel is) checkoljak (nem a dbSaveChanges())
            // talan a FirstOrDefault..? valszeg nem...
            // membership-be beladassal kapcsolatos..?

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

        public async Task SendMessageToGroup(string groupName, string message)
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
        public OperationResult HandleLogIn(string userName)
        {
            // very primitive...
            if (!_userRepository.UserExists(userName))
            {
                _userRepository.Add(new User() { Name = userName });
            }
            return new OperationResult();
        }

        public OperationResult HandleJoinGroup(string userName, string groupName) 
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user != null && group != null)
            {
                if (_groupRepository.CountMembers(group) < 20)
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












