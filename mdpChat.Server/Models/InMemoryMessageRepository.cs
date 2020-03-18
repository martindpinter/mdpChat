using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace mdpChat.Server
{
    public class InMemoryMessageRepository : IMessageRepository
    {
        private List<Message> _messageList;
        public InMemoryMessageRepository()
        {
            _messageList = new List<Message>()
            {
                new Message() 
                { 
                    Id = 1, 
                    GroupId = 1, 
                    SenderName = "TestSender", 
                    MessageBody = "This is my message from InMemoryMessageRepository"
                }
            };
        }

        public Message Add(Message message)
        {
            // set auto increment in decoration?
            message.Id = _messageList.Max(x => x.Id) + 1;
            _messageList.Add(message);

            // DEBUG!
            var json = JsonSerializer.Serialize(_messageList, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine("\n" + json);


            return message;
        }

        public Message DeleteMessageById(int id)
        {
            Message msgToDelete = _messageList.FirstOrDefault(x => x.Id == id);
            if (msgToDelete != null)
            {
                _messageList.Remove(msgToDelete);
            }
            return msgToDelete;
        }

        public IEnumerable<Message> GetAllMessages()
        {
            return _messageList;
        }

        public IEnumerable<Message> GetAllMessagesInGroup(int groupId)
        {
            List<Message> messagesInGroup = _messageList.Where(x => x.GroupId == groupId).ToList();
            return messagesInGroup;
        }

        public Message GetMessage(int id)
        {
            Message resMessage = _messageList.FirstOrDefault(x => x.Id == id);
            return resMessage;
        }

        public Message Update(Message message)
        {
            Message msgToEdit = _messageList.FirstOrDefault(x => x == message);
            if (msgToEdit != null)
            {
                msgToEdit.SenderName = message.SenderName;
                msgToEdit.MessageBody = message.MessageBody;
                msgToEdit.GroupId = message.GroupId;
            }
            return message;
        }
    }
}