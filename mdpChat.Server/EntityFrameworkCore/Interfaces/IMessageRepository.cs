using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IMessageRepository
    {
        void Add(Message message);
        IEnumerable<Message> GetAllMessagesInGroup(int groupId);
    }

}