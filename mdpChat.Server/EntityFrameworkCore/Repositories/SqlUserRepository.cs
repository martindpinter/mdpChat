using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly ChatDbContext _context;
        public SqlUserRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IUserRepository implementation
        public bool UserExists(string userName)
        {
            return _context.Users.Any(x => x.Name == userName);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUser(string userName)
        {
            return _context.Users.FirstOrDefault(x => x.Name == userName);
        }

        public List<User> GetUsersInGroup(string groupName)
        {
            Group group = _context.Groups.FirstOrDefault(x => x.Name == groupName);

            if (group == null)
                return null;

            return _context.Users
                            .Join(_context.Memberships.Where(x => x.GroupId == group.Id),
                                user => user.Id,
                                membership => membership.UserId,
                                (user, membership) => new { tempUser = user, tempMembership = membership })
                            .Select(x => x.tempUser)
                            .ToList();
        }
        #endregion
    }
}