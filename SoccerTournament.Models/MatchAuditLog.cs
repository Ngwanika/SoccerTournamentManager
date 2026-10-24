using System.ComponentModel.DataAnnotations;

namespace SoccerTournament.Models
{
    public sealed class MatchAuditLog
    {
        [Key]
        public int MatchAuditLogId { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        [Required]
        [MaxLength(64)]
        public string Action { get; set; } = "";

        [MaxLength(4000)]
        public string? Details { get; set; }

        [MaxLength(450)]
        public string? ActorUserId { get; set; }

        [MaxLength(256)]
        public string? ActorName { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
