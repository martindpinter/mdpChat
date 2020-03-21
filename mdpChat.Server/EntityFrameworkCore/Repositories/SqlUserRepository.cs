using System.Linq;
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

        public void Add(User user) // AddUnique? 
        {
            user.Id = _context.Users.Max(x => x.Id) + 1;
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUser(string userName)
        {
            return _context.Users.FirstOrDefault(x => x.Name == userName);
        }
        #endregion
    }
}