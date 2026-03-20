using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models.ViewModels
{
    public class TournamentTeamVM
    {
        public int TournamentId { get; set; }

        public Tournament Tournament { get; set; }

        public IEnumerable<Team> AvailableTeams { get; set; }

        public IEnumerable<Team> AddedTeams { get; set; }

        public int SelectedTeamId { get; set; }
    }
}
