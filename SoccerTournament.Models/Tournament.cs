using SoccerTournament.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace SoccerTournament.Models
{
    public class Tournament
    {


        [Key]
        public int TournamentId { get; set; }

        [Required]
        [DisplayName("Tournament Name")]
        public string Name { get; set; }
        [DisplayName(" TournamentSeason")]
        public string Season { get; set; }
        [DisplayName("Location")]
        public string Location { get; set; }
        [DisplayName("Tournament Format")]
        public string? Format { get; set; }
        
        public int PointsWin { get; set; } = 3;
        public int PointsDraw { get; set; } = 1;
        public int PointsLoss { get; set; } = 0;
        [DisplayName("Registration Deadline")]
        public DateTime? DeadlineDate { get; set; }
        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }
        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }
        [DisplayName("Tournament Status")]
        public string Status { get; set; }

        [DisplayName("Max Participants")]
        public int? MaxParticipants { get; set; }

        [DisplayName("Entry Fee")]
        public int? EntryFee { get; set; }
        [DisplayName("Tournament Description")]
        public string? Description { get; set; }
        // public TournamentFormat Format { get; set; }


    }
}
