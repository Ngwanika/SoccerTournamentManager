using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models
{
    public class Goal
    {
        [Key]
        public int GoalId { get; set; }

        [Required]
        public int MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        [Required]
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        [Required]
        public string ScorerName { get; set; } // You can link this to a Player model later

        [Range(1, 120)]
        public int Minute { get; set; }

        public bool IsPenalty { get; set; }
    }
}
