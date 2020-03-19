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
        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        #endregion
    }
}