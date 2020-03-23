using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server.EntityFrameworkCore.TableRows
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(12, ErrorMessage = "Users' name must be max 12 characters long")]
        public string Name { get; set; }

        // necessary?
        // public bool IsOnline { get; set; }
    }
}