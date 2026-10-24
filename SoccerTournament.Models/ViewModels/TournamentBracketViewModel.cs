namespace SoccerTournament.Models.ViewModels
{
    public sealed class TournamentBracketViewModel
    {
        public int TournamentId { get; init; }
        public string TournamentName { get; init; } = "";
        public string Format { get; init; } = "";
        public bool HasDraw { get; init; }
        public IReadOnlyList<int> DrawOrder { get; init; } = [];
        public IReadOnlyList<BracketTeamViewModel> Teams { get; init; } = [];
        public IReadOnlyList<BracketGroupViewModel> Groups { get; init; } = [];
        public IReadOnlyList<BracketRoundViewModel> Rounds { get; init; } = [];
        public IReadOnlyList<LeagueStandingViewModel> Standings { get; init; } = [];
        public int MatchesGenerated { get; init; }
    }

    public sealed class BracketTeamViewModel
    {
        public int TeamId { get; init; }
        public string Name { get; init; } = "";
    }

    public sealed class BracketGroupViewModel
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<BracketTeamViewModel> Teams { get; init; } = [];
        public IReadOnlyList<LeagueStandingViewModel> Standings { get; init; } = [];
    }

    public sealed class BracketRoundViewModel
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<BracketMatchViewModel> Matches { get; init; } = [];
    }

    public sealed class BracketMatchViewModel
    {
        public int? MatchId { get; init; }
        public string HomeTeam { get; init; } = "TBD";
        public string AwayTeam { get; init; } = "TBD";
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public string Status { get; init; } = "TBD";
    }

    public sealed class LeagueStandingViewModel
    {
        public int Position { get; init; }
        public int TeamId { get; init; }
        public string TeamName { get; init; } = "";
        public int Played { get; init; }
        public int Won { get; init; }
        public int Drawn { get; init; }
        public int Lost { get; init; }
        public int GoalsFor { get; init; }
        public int GoalsAgainst { get; init; }
        public int Points { get; init; }
    }
}
