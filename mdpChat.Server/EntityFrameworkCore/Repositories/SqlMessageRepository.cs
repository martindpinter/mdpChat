using System.Collections.Generic;
using System.Linq;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Repositories
{
    public class SqlMessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public SqlMessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IMessageRepository implementation
        public void Add(Message message)
        {
            _context.Messages.Add(message);
            _context.SaveChanges();
        }

        public List<Message> GetAllMessagesInGroup(string groupName)
        {
            Group group = _context.Groups.FirstOrDefault(x => x.Name == groupName);
            return _context.Messages.Where(x => x.GroupId == group.Id).ToList();
        }
        #endregion
    }
}