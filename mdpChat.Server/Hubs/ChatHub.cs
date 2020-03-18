using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        public ChatHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;           
        }

        public async Task JoinGroup(string groupName)
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
            _messageRepository.Add(new Message()
            {
                SenderName = Context.ConnectionId,
                MessageBody = message,
                GroupId = 1 // TODO 
            });

            await Clients.Group(groupName).SendAsync(message);
        }

        public async Task SendMessageToAll(string user, string message)
        {
            _messageRepository.Add(new Message()
            {
                SenderName = Context.ConnectionId,
                MessageBody = message,
                GroupId = 1 // TODO 
            });

            // trusting the user's name from the client will do for now...
            string msg = $"[{ user }]: { message } ";
            await Clients.All.SendAsync("ReceiveMessage", msg);
        }

        public async Task RequestUserName() 
        {
            // ConnectionId will not be exposed later on, only debug functionality for now
            await Clients.Caller.SendAsync("ReceiveUserName", Context.ConnectionId);
        }

        public async override Task OnConnectedAsync()
        {
            // Global chat is considered a group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Global");

            // update clients...

            // notify users in group (factor)
            string message = Context.ConnectionId + " joined the chat.";
            await SendMessageToAll("SYSTEM", message);

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