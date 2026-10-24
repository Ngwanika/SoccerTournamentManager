using System.ComponentModel.DataAnnotations;

namespace SoccerTournament.Models
{
    public sealed class TournamentDraw
    {
        [Key]
        public int TournamentDrawId { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        [Required]
        public string TeamOrderJson { get; set; } = "[]";

        public string? KnockoutTeamOrderJson { get; set; }

        public DateTime FinalizedAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? FinalizedByUserId { get; set; }
    }
}
