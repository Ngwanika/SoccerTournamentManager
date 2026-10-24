using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SoccerTournament.Models
{
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int JerseyNumber { get; set; }
        public string Position { get; set; }
        // Suggestion: Identity verification for league rules
        public string? NationalIdNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlayerPhotoUrl { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }

        // FOREIGN KEY
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}
