using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models.ViewModels
{
    public class LiveScoreVM
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public IEnumerable<Match> Matches { get; set; }
    }
}
