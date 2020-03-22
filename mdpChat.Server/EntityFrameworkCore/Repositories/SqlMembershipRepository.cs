using System.Collections.Generic;
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

        public List<Membership> GetMembershipsInGroup(Group group)
        {
            return _context.Memberships.Where(x => x.GroupId == group.Id).ToList();
        }

        public bool MembershipExists(User user, Group group)
        {
            return _context.Memberships.Any(x => x.UserId == user.Id && x.GroupId == group.Id);
        }

        public void Add(Membership membership)
        {
            _context.Memberships.Add(membership);
            _context.SaveChanges();
        }

        public void Remove(Membership membership)
        {
            // VERIFY!
            Membership membershipToRemove = _context.Memberships.FirstOrDefault(x => x.UserId == membership.UserId && x.GroupId == membership.GroupId);
            _context.Memberships.Remove(membershipToRemove);
            _context.SaveChanges();
        }

        public void RemoveAllForGroup(Group group)
        {
            // VERIFY!
            List<Membership> membershipsToRemove = _context.Memberships.Where(x => x.GroupId == group.Id).ToList();
            _context.Memberships.RemoveRange(membershipsToRemove);
            _context.SaveChanges();
        }
        #endregion
    }
}

