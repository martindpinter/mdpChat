using System.Collections.Generic;

namespace mdpChat.Server
{
    public interface IMessageRepository
    {
        Message GetMessage(int id);
        IEnumerable<Message> GetAllMessages();
        IEnumerable<Message> GetAllMessagesInGroup(int groupId);
        Message Add(Message message);

        // test
        Message Update(Message message);
        // Message DeleteMessage(Message message);
        Message DeleteMessageById(int id);
        
    }

}