using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server.EntityFrameworkCore.TableRows
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        public int? UserIdAssigned { get; set; } 
    }
}