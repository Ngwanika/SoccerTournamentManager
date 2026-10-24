using System.ComponentModel.DataAnnotations;

namespace SoccerTournament.Models
{
    public sealed class TeamContactMessage
    {
        [Key]
        public int TeamContactMessageId { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        [Required]
        [MaxLength(450)]
        public string SenderUserId { get; set; } = "";

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string ReplyEmail { get; set; } = "";

        [Required]
        [MaxLength(160)]
        public string Subject { get; set; } = "";

        [Required]
        [MaxLength(5000)]
        public string Message { get; set; } = "";

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }
    }
}
