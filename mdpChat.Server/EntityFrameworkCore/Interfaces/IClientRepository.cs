using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IClientRepository
    {
        void Add(Client client);
        void UpdateAsNew(Client client);
        void Remove(Client client);
        Client GetClient(string connectionId);
        List<Client> GetClientsAssignedToUser(User user);
        List<Client> GetClientsInGroup(string groupName);
        void AssignUser(string connectionId, User user);
        bool ClientExists(string connectionId);
        bool HasUserAttached(string connectionId);
        User GetUserAttached(string connectionId);
    }
}