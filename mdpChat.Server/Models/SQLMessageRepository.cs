using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace mdpChat.Server
{
    public class SQLMessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public SQLMessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        #region IMessageRepository implementation
        public Message Add(Message message)
        {
            _context.Messages.Add(message);
            _context.SaveChanges();
            return message;
        }

        public Message DeleteMessageById(int id)
        {
            Message msgToDelete = _context.Messages.Find(id);
            if (msgToDelete != null)
            {
                _context.Messages.Remove(msgToDelete);
                _context.SaveChanges();
            }
            return msgToDelete;
        }

        public IEnumerable<Message> GetAllMessages()
        {
            return _context.Messages;
        }

        public IEnumerable<Message> GetAllMessagesInGroup(int groupId)
        {
            // return _context.Messages.
            throw new System.NotImplementedException();
        }

        public Message GetMessage(int id)
        {
            return _context.Messages.Find(id);
        }

        public Message Update(Message message)
        {
            var msgToUpdate = _context.Messages.Attach(message);
            msgToUpdate.State = EntityState.Modified;

            _context.SaveChanges();
            
            return message;
        }
        #endregion
    }
}