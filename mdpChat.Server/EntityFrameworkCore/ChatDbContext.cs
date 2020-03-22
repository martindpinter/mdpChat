using mdpChat.Server.EntityFrameworkCore.TableRows;
using Microsoft.EntityFrameworkCore;

namespace mdpChat.Server.EntityFrameworkCore
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Group>().HasData(
                new Group() {
                    Id = 1,
                    Name = "Global"
                }
            );
        }
    }
}