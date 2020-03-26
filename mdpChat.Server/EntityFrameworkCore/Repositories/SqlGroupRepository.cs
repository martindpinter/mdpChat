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
            if (_context.Groups.Any(x => x.Name == group.Name))
                return;

            group.Capacity = 20;
            _context.Groups.Add(group);
            _context.SaveChanges();
        }

        public Group GetGroup(string groupName)
        {
            return _context.Groups.FirstOrDefault(x => x.Name == groupName);
        }

        public Group GetGroup(int id)
        {
            return _context.Groups.FirstOrDefault(x => x.Id == id);
        }

        public List<Group> GetAllGroups()
        {
            return _context.Groups.ToList();
        }

        public bool IsFull(Group group)
        {
            if (group.Capacity == null)
                return false; // Global channel has unlimited capacity

            int count = _context.Memberships.Where(x => x.GroupId == group.Id).Count();
            return count >= group.Capacity; // TODO - define max count in configuration
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
