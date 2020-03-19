using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server.EntityFrameworkCore.TableRows
{
    public class Membership
    {
        public int Id { get; set; } // EFCore requires PK to sync changes

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GroupId { get; set; }
    }
}