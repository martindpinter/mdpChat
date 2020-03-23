using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server.EntityFrameworkCore.TableRows
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [MaxLength(2000, ErrorMessage = "Messages must be max 2000 characters long")]
        public string MessageBody { get; set; }

        [Required]
        public int GroupId { get; set; }
    }
}