using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IMembershipRepository
    {
        Membership GetMembership(User user, Group group);
        bool MembershipExists(User user, Group group);
        void Add(Membership membership);
        void Remove(Membership membership);
    }
}
