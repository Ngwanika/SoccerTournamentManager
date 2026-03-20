using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models
{
    public class Group
    {

        public int GroupId { get; set; }

        public string GroupName { get; set; } // Group A, Group B

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

    }
}
