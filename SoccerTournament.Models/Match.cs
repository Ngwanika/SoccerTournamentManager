using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoccerTournament.Models
{
    public class Match
    {
        [Key]
        public int MatchId { get; set; }

        public int TournamentId { get; set; }

        public DateTime MatchDate { get; set; }

        public string? Stage { get; set; }   // Group, Quarter Final, Semi Final, Final

        public string? Status { get; set; }  // Scheduled, Live, Finished

        public int? HomeScore { get; set; }

        public int? AwayScore { get; set; }

        [ForeignKey(nameof(HomeTeam))]
        public int HomeTeamId { get; set; }
        public Team? HomeTeam { get; set; }

        //[ForeignKey(nameof(AwayTeam))]
        public int AwayTeamId { get; set; }
        public Team? AwayTeam { get; set; }

        public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    }
}