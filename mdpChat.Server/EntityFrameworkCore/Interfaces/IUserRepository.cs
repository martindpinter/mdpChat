using mdpChat.Server.EntityFrameworkCore.TableRows;

namespace mdpChat.Server.EntityFrameworkCore.Interfaces
{
    public interface IUserRepository
    {
        void Add(User user);
    }
}