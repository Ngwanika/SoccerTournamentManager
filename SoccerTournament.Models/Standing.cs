using System.ComponentModel.DataAnnotations;


namespace SoccerTournament.Models
{
    public class Standing
    {

        [Key]
        public int StandingId { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }

        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }

        public int Points { get; set; }
    }
}
