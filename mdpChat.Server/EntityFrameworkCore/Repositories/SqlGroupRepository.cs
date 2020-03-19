using System.Collections.Generic;
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

        public IEnumerable<Group> GetAllGroups()
        {
            return _context.Groups;
        }
        #endregion
    }
}
