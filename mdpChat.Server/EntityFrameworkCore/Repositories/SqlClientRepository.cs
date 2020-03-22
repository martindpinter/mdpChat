using System;
using System.Collections.Generic;
using System.Linq;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Repositories
{
    public class SqlClientRepository : IClientRepository
    {
        private readonly ChatDbContext _context;
        public SqlClientRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IClientRepository implementation
        public void Add(Client client)
        {
            client.Id = _context.Clients.Any() ? _context.Clients.Max(x => x.Id) + 1 : 1;
            client.UserIdAssigned = 999;
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        public Client GetClient(string connectionId)
        {
            return _context.Clients.FirstOrDefault(x => x.ConnectionId == connectionId);
        }

        public List<Client> GetClientsAssignedToUser(User user)
        {
            return _context.Clients.Where(x => x.UserIdAssigned == user.Id).ToList();
        }

        public List<Client> GetClientsInGroup(string groupName)
        {
            Group group = _context.Groups.FirstOrDefault(x => x.Name == groupName);
            if (group != null)
            {
                List<Membership> membershipsInGroup = _context.Memberships.Where(x => x.GroupId == group.Id).ToList();
                return _context.Clients.Where(x => membershipsInGroup.Any(y => y.UserId == x.UserIdAssigned)).ToList();
            }
            return null;
        }

        public void AssignUser(string connectionId, User user)
        {
            // attach?
            Client client = _context.Clients.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (client != null)
            {
                client.UserIdAssigned = user.Id;
                _context.SaveChanges();
            }
        }

        public bool ClientExists(string connectionId)
        {
            return _context.Clients.Any(x => x.ConnectionId == connectionId);
        }

        public bool HasUserAttached(string connectionId)
        {
            Client client = GetClient(connectionId);
            return !String.IsNullOrEmpty(client.ConnectionId);
        }

        public User GetUserAttached(string connectionId)
        {
            // revise!
            Client client = _context.Clients.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (client != null)
            {
                return _context.Users.FirstOrDefault(x => x.Id == client.UserIdAssigned);
            }
            return null;
        }
        #endregion
    }
}