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
        public void Add(Membership membership)
        {
            _context.Memberships.Add(membership);
            _context.SaveChanges();
        }
        #endregion
    }
}

