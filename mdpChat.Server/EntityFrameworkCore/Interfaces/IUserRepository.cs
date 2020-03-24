using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IUserRepository
    {
        bool UserExists(string userName);
        
        void Add(User user);
        
        User GetUser(string name);
        
        List<User> GetUsersInGroup(string groupName);
    }
}