using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models
{
    public class TournamentTeam
    {
        public int TournamentTeamId { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

    }
}
