namespace SoccerTournament.Models.ViewModels
{
    public class TournamentCardVM
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public string Season { get; set; }
        public string Location { get; set; }

        public DateTime? DeadlineDate { get; set; }
        public DateTime? StartDate { get; set; }

        public string TournamentStatus { get; set; }  // Upcoming, Active, Completed

        // Registration Info
        public TournamentRegistration? RegistrationStatus { get; set; }

        public bool IsPaid { get; set; }

        //Computed Properties(UI Logic)
        //public bool CanApply =>
        //    TournamentStatus == "Upcoming"
        //    && RegistrationStatus == null
        //    && DeadlineDate >= DateTime.Now;

        //public bool IsPending =>
        //    RegistrationStatus == RegistrationStatus.Pending;

        //public bool IsApproved =>
        //    RegistrationStatus == RegistrationStatus.Approved;

        //public bool IsRejected =>
        //    RegistrationStatus == RegistrationStatus.Rejected;

        //public bool IsPlaying =>
        //    TournamentStatus == "Active"
        //    && RegistrationStatus == RegistrationStatus.Approved;
    }
}