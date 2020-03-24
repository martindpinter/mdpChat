using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IGroupRepository
    {
        void Add(Group group);

        Group GetGroup(string groupName);

        List<Group> GetAllGroups();

        bool IsFull(Group group);

        int CountMembers(Group group);

        bool GroupExists(string groupName);
    }
}
