using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoccerTournament.Models
{
    public class MatchEvent
    {

        [Key]
        public int EventId { get; set; }

        public int MatchId { get; set; }
        [ForeignKey(nameof(MatchId))]
        public Match Match { get; set; } = null!;

        public int? PlayerId { get; set; }
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        public int? TeamId { get; set; }
        [ForeignKey(nameof(TeamId))]
        public Team? Team { get; set; }

        [Required]
        [MaxLength(32)]
        public string EventType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int EventMinute { get; set; }

        public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? RecordedByUserId { get; set; }
    }
}
