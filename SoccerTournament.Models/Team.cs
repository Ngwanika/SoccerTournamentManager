using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.ComponentModel;

namespace SoccerTournament.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        [DisplayName("Team Name")]
        public string TeamName { get; set; }

        [MaxLength(3), DisplayName("Short Name")] // For scoreboards like "ARS" or "CHE"
        public string? Abbreviation { get; set; }

        [DisplayName("Team Logo")]
        public string? LogoUrl { get; set; }
        [DisplayName("Team couch")]
        public string CoachName { get; set; }
        [DisplayName("Email Address")]
        public string? Email { get; set; }
        [DisplayName("Home Stadium")]
        public string? Homestadium { get; set; }
        [DisplayName("Contact Number 1")]
        public string? ContactNumber1 { get; set; }
        [DisplayName("Contact Number 2")]
        public string? ContactNumber2 { get; set; }

        [DisplayName("Established Date")]
        public DateTime? Established { get; set; }

        // RELATIONSHIPS
        // 1 Team -> Many Players (Your requirement: 0 or >1)
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();

        // Many Teams <-> Many Tournaments
        public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
