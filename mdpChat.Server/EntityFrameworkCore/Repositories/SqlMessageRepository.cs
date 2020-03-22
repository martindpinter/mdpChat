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
            message.Id = _context.Messages.Any() ? _context.Messages.Max(x => x.Id) + 1 : 1;
            _context.Messages.Add(message);
            _context.SaveChanges();
        }

        public IEnumerable<Message> GetAllMessagesInGroup(int groupId)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}