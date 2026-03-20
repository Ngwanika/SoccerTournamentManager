using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models.ViewModels
{
    public class TeamDashboardVM
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }

        public List<TournamentCardVM> Tournaments { get; set; } = new();
    }
}
