using System.Collections.Generic;
using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IGroupRepository
    {
        void Add(Group group);

        IEnumerable<Group> GetAllGroups();
    }
}
