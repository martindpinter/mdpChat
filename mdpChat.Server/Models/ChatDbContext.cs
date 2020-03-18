using Microsoft.EntityFrameworkCore;

namespace mdpChat.Server
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            
        }

        public DbSet<Message> Messages { get; set; }
    }
}