using System.ComponentModel.DataAnnotations;

namespace SoccerTournament.Models
{
    public class MatchEvent
    {

        [Key]
        public int EventId { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; }

        public int? PlayerId { get; set; }
        public Player Player { get; set; }

        public string EventType { get; set; }
        public int EventMinute { get; set; }
    }
}
