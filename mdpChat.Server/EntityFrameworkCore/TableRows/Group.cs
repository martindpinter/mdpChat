using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server.EntityFrameworkCore.TableRows
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(16, ErrorMessage = "Groups' name must be max 16 characters long")]
        public string Name { get; set; } 

    }
}