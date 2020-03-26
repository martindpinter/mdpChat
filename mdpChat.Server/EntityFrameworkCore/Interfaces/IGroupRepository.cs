using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IGroupRepository
    {
        void Add(Group group);

        Group GetGroup(int id);

        Group GetGroup(string groupName);

        List<Group> GetAllGroups();

        List<string> GetAllGroupNames();

        bool IsFull(Group group);

        int CountMembers(Group group);

        bool GroupExists(string groupName);
    }
}
