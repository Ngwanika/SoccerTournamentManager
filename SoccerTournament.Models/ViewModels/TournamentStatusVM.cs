using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.Models.ViewModels
{
    public class TournamentStatusVM
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// null  = never applied
        /// "Pending"  = applied, awaiting admin review
        /// "Approved" = admin accepted the team
        /// "Rejected" = admin rejected the team (can re-apply)
        /// </summary>
        public string? Status { get; set; }

        // ── Convenience helpers used in the Razor view ──────────────────────
        public bool HasApplied => Status != null;
        public bool CanApply => Status == null || Status == "Rejected";
        public bool IsDeadlinePassed => Deadline.HasValue && Deadline.Value < DateTime.Now;

    }
}
