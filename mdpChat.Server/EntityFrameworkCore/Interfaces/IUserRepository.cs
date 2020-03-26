using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IUserRepository
    {
        bool UserExists(string userName);
        
        void Add(User user);
        
        User GetUser(int id);
        User GetUser(string name);
        
        List<User> GetUsersInGroup(string groupName);

        List<string> GetUserNamesInGroup(string groupName);

        void SetUserOnline(string userName);
        void SetUserOffline(string userName);
        void SetUserOffline(int userId);
    }
}