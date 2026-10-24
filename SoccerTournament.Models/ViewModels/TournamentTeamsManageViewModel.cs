namespace SoccerTournament.Models.ViewModels
{
    public sealed class TournamentTeamsManageViewModel
    {
        public int TournamentId { get; init; }
        public string TournamentName { get; init; } = "";
        public IReadOnlyList<TournamentTeamRowViewModel> ParticipatingTeams { get; init; } = [];
        public IReadOnlyList<TournamentTeamRowViewModel> AvailableTeams { get; init; } = [];
        public int MatchCount { get; init; }
        public bool DrawFinalized { get; init; }
    }

    public sealed class TournamentTeamRowViewModel
    {
        public int TeamId { get; init; }
        public string TeamName { get; init; } = "";
        public int PlayerCount { get; init; }
        public DateTime? JoinedAt { get; init; }
        public bool HasFixtures { get; init; }
    }

    public sealed class TeamCommunicationViewModel
    {
        public required IReadOnlyList<TeamMemberEmailViewModel> Members { get; init; }
        public string? WhatsAppGroupLink { get; init; }
    }

    public sealed class TeamMemberEmailViewModel
    {
        public int PlayerId { get; init; }
        public string Name { get; init; } = "";
        public string? Email { get; init; }
    }

    public sealed class TeamEmailRequest
    {
        public int? PlayerId { get; set; }
        public bool SendToAll { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(160)]
        public string Subject { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(5000)]
        public string Message { get; set; } = "";
    }

    public sealed class TeamContactAdminRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(160)]
        public string Subject { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(5000)]
        public string Message { get; set; } = "";
    }
}
