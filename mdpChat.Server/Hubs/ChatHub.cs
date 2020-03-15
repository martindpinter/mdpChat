using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace mdpChat.Server
{
    public class ChatHub : Hub
    {
        public async Task SendMessageToAll(string user, string message)
        {
            // trusting the user's name from the client will do for now...
            string msg = $"[{ user }]: { message } ";
            await Clients.All.SendAsync("ReceiveMessage", msg);
        }

        public async Task RequestUserName() 
        {
            await Clients.Caller.SendAsync("ReceiveUserName", Context.ConnectionId);
        }

        public async override Task OnConnectedAsync()
        {
            string message = Context.ConnectionId + " joined the chat.";
            await SendMessageToAll("SYSTEM", message);
        }
    }
}