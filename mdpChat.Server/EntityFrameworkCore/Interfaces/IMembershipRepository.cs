using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IMembershipRepository
    {
        void Add(Membership membership);
    }
}
