using System.Linq;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Repositories
{
    public class SqlMembershipRepository : IMembershipRepository
    {
        private readonly ChatDbContext _context;
        public SqlMembershipRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IMembershipRepository implementation
        public Membership GetMembership(User user, Group group)
        {
            return _context.Memberships.FirstOrDefault(x => x.UserId == user.Id && x.GroupId == group.Id);
        }

        public bool MembershipExists(User user, Group group)
        {
            return _context.Memberships.Any(x => x.UserId == user.Id && x.GroupId == group.Id);
        }

        public void Add(Membership membership)
        {
            membership.Id = _context.Memberships.Max(x => x.Id) + 1;
            _context.Memberships.Add(membership);
            _context.SaveChanges();
        }

        public void Remove(Membership membership)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}

