namespace SoccerTournament.Models.ViewModels
{
    public sealed class MatchOfficialWorkspaceViewModel
    {
        public required Match Match { get; init; }
        public required string TournamentName { get; init; }
        public IReadOnlyList<Player> Players { get; init; } = [];
        public IReadOnlyList<MatchEvent> Events { get; init; } = [];
        public IReadOnlyList<MatchAuditLog> AuditHistory { get; init; } = [];
    }

    public sealed class MatchEventRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(32)]
        public string EventType { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Range(0, 150)]
        public int EventMinute { get; set; }

        public int TeamId { get; set; }
        public int? PlayerId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class MatchDetailsViewModel
    {
        public int MatchId { get; init; }
        public int TournamentId { get; init; }
        public string TournamentName { get; init; } = "";
        public string Status { get; init; } = "";
        public string? Stage { get; init; }
        public DateTime MatchDate { get; init; }
        public int HomeTeamId { get; init; }
        public string HomeTeamName { get; init; } = "";
        public int AwayTeamId { get; init; }
        public string AwayTeamName { get; init; } = "";
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public string? LeadOfficialName { get; init; }
        public string? AssistantOfficialName { get; init; }
        public bool HasTrackedEvents { get; init; }
        public int? HomeCorners { get; init; }
        public int? AwayCorners { get; init; }
        public int? HomeOffsides { get; init; }
        public int? AwayOffsides { get; init; }
        public int? HomePenalties { get; init; }
        public int? AwayPenalties { get; init; }
        public int PreviousMeetings { get; init; }
        public int HomeWins { get; init; }
        public int AwayWins { get; init; }
        public int Draws { get; init; }
        public IReadOnlyList<MatchPlayerViewModel> HomePlayers { get; init; } = [];
        public IReadOnlyList<MatchPlayerViewModel> AwayPlayers { get; init; } = [];
        public IReadOnlyList<MatchEventViewModel> Events { get; init; } = [];
    }

    public sealed class MatchPlayerViewModel
    {
        public int PlayerId { get; init; }
        public int TeamId { get; init; }
        public string Name { get; init; } = "";
        public int JerseyNumber { get; init; }
        public string? Position { get; init; }
    }

    public sealed class MatchEventViewModel
    {
        public string EventType { get; init; } = "";
        public string TeamName { get; init; } = "";
        public string? PlayerName { get; init; }
        public int Minute { get; init; }
        public string? Notes { get; init; }
    }
}
