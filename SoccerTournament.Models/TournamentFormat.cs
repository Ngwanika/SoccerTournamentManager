using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models
{
    public class TournamentFormat
    {
        public int TournamentFormatId { get; set; }
        public string FormatName { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
