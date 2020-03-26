using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.Interfaces;
using mdpChat.Server.EntityFrameworkCore.TableRows;
using mdpChat.Server.Models;

namespace mdpChat.Server
{
    public interface IDataManager
    {
        User GetUserAttached(string connectionId);
        List<User> GetUsersInGroup(string groupName);
        User GetUser(int id);
        User GetUser(string userName);
        Group GetGroup(int id);
        Group GetGroup(string groupname);
        List<Group> GetAllGroups();
        List<Client> GetClientsInGroup(string groupName);
        List<Message> GetAllMessagesInGroup(string groupName);
        bool MembershipExists(User user, Group group);
        void AddMembership(Membership membership);
        bool IsGroupFull(Group group);


        OperationResult HandleConnection(string connectionId);
        OperationResult HandleDisconnection(string connectionId);
        OperationResult HandleLogin(string connectionId, string userName);
        OperationResult HandleCreateGroup(string groupName);
        OperationResult HandleDeleteGroup(string groupName);
        OperationResult HandleJoinGroup(string userName, string groupName); // revert params order
        OperationResult HandleLeaveGroup(User user, Group group); // revert params order
        OperationResult HandleSendMessageToGroup(string groupName, string message, string connectionId);
        
    }
    public class DataManager : IDataManager
    {
        private readonly string _globalChatRoomName = "General"; // move to config?

        private IUserRepository _userRepository;
        private IClientRepository _clientRepository;
        private IGroupRepository _groupRepository;
        private IMembershipRepository _membershipRepository;
        private IMessageRepository _messageRepository;

        public DataManager(IUserRepository userRepository,
                            IClientRepository clientRepository,
                            IGroupRepository groupRepository,
                            IMembershipRepository membershipRepository,
                            IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _groupRepository = groupRepository;
            _membershipRepository = membershipRepository;
            _messageRepository = messageRepository;
        }

        #region Repository passthrough
        public User GetUserAttached(string connectionId) 
            => _clientRepository.GetUserAttached(connectionId); // move to userrepo? 

        public List<User> GetUsersInGroup(string groupName) 
            => _userRepository.GetUsersInGroup(groupName);

        public User GetUser(int id)
            => _userRepository.GetUser(id);

        public User GetUser(string userName)
            => _userRepository.GetUser(userName);

        public Group GetGroup(int id)
            => _groupRepository.GetGroup(id);

        public Group GetGroup(string groupName) 
            => _groupRepository.GetGroup(groupName);

        public List<Group> GetAllGroups() 
            => _groupRepository.GetAllGroups();

        public List<Client> GetClientsInGroup(string groupName) 
            => _clientRepository.GetClientsInGroup(groupName);

        public List<Message> GetAllMessagesInGroup(string groupName) 
            => _messageRepository.GetAllMessagesInGroup(groupName);

        public bool MembershipExists(User user, Group group)
            => _membershipRepository.MembershipExists(user, group);

        public void AddMembership(Membership membership)
            => _membershipRepository.Add(membership);

        public bool IsGroupFull(Group group)
            => _groupRepository.IsFull(group);
        #endregion



        public OperationResult HandleConnection(string connectionId)
        {
            Client existingClient = _clientRepository.GetClient(connectionId);

            if (existingClient == null)
            {
                _clientRepository.Add(new Client() { ConnectionId = connectionId });
            }
            else
            {
                // make old connectionId vacant for new client
                _clientRepository.UpdateAsNew(existingClient);
            }
            return new OperationResult();
        }

        public OperationResult HandleDisconnection(string connectionId)
        {
            Client client = _clientRepository.GetClient(connectionId);
            
            if (client == null)
                return new OperationResult() { ErrorMessage = $"Error disconnecting: no client exists with ConnectionId \"{ connectionId }\"" }; // not going to show ever, why do I even bother? exceptions pls...?

            int? assignedUserId = client.UserIdAssigned;
            _clientRepository.Remove(client);

            // Mark user as Offline if all connected clients were removed
            if (assignedUserId != null)
            {
                if (_clientRepository.CountUserConnections((int)assignedUserId) == 0)
                {
                    _userRepository.SetUserOffline((int)assignedUserId);
                }
            }

            return new OperationResult();
        }

        public OperationResult HandleLogin(string connectionId, string userName)
        {
            User user = _userRepository.GetUser(userName);

            if (user == null)
            {
                _userRepository.Add(new User() { Name = userName });
                user = _userRepository.GetUser(userName);  // not nice :(
            }


            _clientRepository.AssignUser(connectionId, user);
            _userRepository.SetUserOnline(user.Name);

            OperationResult res = HandleJoinGroup(userName, _globalChatRoomName);
            if (!res.Successful)
                return res;

            return new OperationResult();
        }

        public OperationResult HandleJoinGroup(string userName, string groupName) 
        {
            User user = _userRepository.GetUser(userName);
            Group group = _groupRepository.GetGroup(groupName);

            if (user == null)
                return new OperationResult() { ErrorMessage = $"Error joining group: no user exists with name \"{ userName }\"" };

            if (group == null) 
            {
                _groupRepository.Add(new Group() { Name = groupName });
                group = _groupRepository.GetGroup(groupName); // not nice, again :(  (return Group?)
            }

            if (_groupRepository.IsFull(group)) 
                return new OperationResult() { ErrorMessage = $"Error joining group: group \"{ group.Name }\" is full" };

            _membershipRepository.Add(new Membership()
            {
                UserId = user.Id,
                GroupId = group.Id
            });
            return new OperationResult();
        }

        public OperationResult HandleLeaveGroup(User user, Group group)
        {
            Membership membership = _membershipRepository.GetMembership(user, group);

            if (user == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: user is null" };

            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: group is null" };

            if (membership == null)
                return new OperationResult() { ErrorMessage = $"Error leaving group: user \"{ user.Name }\" is not a member of group \"{ group.Name }\"" };

            _membershipRepository.Remove(new Membership()
            {
                UserId = user.Id,
                GroupId = group.Id
            });
            return new OperationResult();
        }

        public OperationResult HandleCreateGroup(string groupName)
        {
            // TODO!!!!
            _groupRepository.Add(new Group()
            {
                Name = groupName
            });
            return new OperationResult();
        }

        public OperationResult HandleDeleteGroup(string groupName)
        {
            Group group = _groupRepository.GetGroup(groupName);
            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error deleting group: no group exists with name \"{ groupName } \"" };

            _membershipRepository.RemoveAllForGroup(group);
            return new OperationResult();
        }

        public OperationResult HandleSendMessageToGroup(string groupName, string message, string connectionId)
        {
            User user = _clientRepository.GetUserAttached(connectionId);
            Group group = _groupRepository.GetGroup(groupName);

            if (user == null)
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ groupName }\": no user is bound to current connection" };

            if (group == null)
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ groupName }\": no such group exists" };

            if (!_membershipRepository.MembershipExists(user, group))
                return new OperationResult() { ErrorMessage = $"Error sending message to group \"{ group.Name }\": User \"{ user.Name }\" is not a member of the group" };

            _messageRepository.Add(new Message
            {
                AuthorId = user.Id,
                GroupId = group.Id,
                MessageBody = message,
            });
            return new OperationResult();
        }
    }
}