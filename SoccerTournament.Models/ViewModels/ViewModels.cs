using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models.ViewModels
{
    public class TournamentBracketsVM
    {
        // Metadata for the header
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }

        // We use a helper class "MatchDetail" to avoid confusion 
        // with the base "Match" database entity
        public IEnumerable<BracketMatchDetail> SemiFinals { get; set; }
        public BracketMatchDetail Final { get; set; }
    }

    public class BracketMatchDetail
    {
        // Using "Name" strings here is better for the View 
        // than using IDs, so the View doesn't have to do logic
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
