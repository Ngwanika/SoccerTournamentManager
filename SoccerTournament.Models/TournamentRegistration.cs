using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models
{
    public class TournamentRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }

        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public DateTime? ReviewedDate { get; set; }

        public string? AdminNotes { get; set; }

    }
}
