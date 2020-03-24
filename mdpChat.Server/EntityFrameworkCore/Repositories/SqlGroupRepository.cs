using System.Collections.Generic;
using System.Linq;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Repositories
{
    public class SqlGroupRepository : IGroupRepository
    {
        private readonly ChatDbContext _context;
        public SqlGroupRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IGroupRepository implementation
        public void Add(Group group)
        {
            _context.Groups.Add(group);
            _context.SaveChanges();
        }

        public Group GetGroup(string groupName)
        {
            return _context.Groups.FirstOrDefault(x => x.Name == groupName);
        }

        public List<Group> GetAllGroups()
        {
            return _context.Groups.ToList();
        }

        public bool IsFull(Group group)
        {
            // DEBUG HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return false;

            // int count = _context.Memberships.Where(x => x.GroupId == group.Id).Count();
            // return count >= 20; // TODO - define max count in configuration
        }

        public int CountMembers(Group group)
        {
            return _context.Memberships.Where(x => x.GroupId == group.Id).Count();
        }

        public bool GroupExists(string groupName)
        {
            return (_context.Groups.FirstOrDefault(x => x.Name == groupName) != null);
        }
        #endregion
    }
}
