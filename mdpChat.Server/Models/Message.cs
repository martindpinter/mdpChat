using System.ComponentModel.DataAnnotations;

namespace mdpChat.Server
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Sender's name must be max 100 characters")] // not 100 obv
        public string SenderName { get; set; }

        [Required]
        [MaxLength(6000, ErrorMessage = "Messages must be max 6000 characters")]
        public string MessageBody { get; set; }

        [Required]
        public int GroupId { get; set; }
    }
}