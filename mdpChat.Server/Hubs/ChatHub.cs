using System;
using System.Threading.Tasks;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;
using Microsoft.AspNetCore.SignalR;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IMembershipRepository _membershipRepository;

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

        public async Task JoinGroup(string groupName) // none of these will be async, probably
        {
            // if group.size <= 20 
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // update clients...
        }

        public async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(message);
        }

        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
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
    }
}