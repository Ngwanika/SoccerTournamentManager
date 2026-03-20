// SoccerTournament.Models/Models/ViewModels/TournamentMatchesVM.cs

namespace SoccerTournament.Models.ViewModels
{
    /// <summary>
    /// Admin view: all matches in a single tournament, grouped by stage.
    /// No team filter — every match is shown.
    /// </summary>
    public class TournamentMatchesVM
    {
        public int    TournamentId   { get; set; }
        public string TournamentName { get; set; } = "";
        public string TournamentStatus { get; set; } = "";

        public string? TeamName { get; set; }
        public int? TeamId { get; set; }

        // Pre-grouped and ordered by the controller
        public List<AdminMatchStageGroup> GroupedMatches { get; set; } = new();

        // Counts for the header stats bar
        public int TotalMatches    => GroupedMatches.Sum(g => g.Matches.Count);
        public int CompletedCount  => GroupedMatches.Sum(g => g.Matches.Count(m => m.Status == "Completed"));
        public int LiveCount       => GroupedMatches.Sum(g => g.Matches.Count(m => m.Status == "Live"));
        public int UpcomingCount   => GroupedMatches.Sum(g => g.Matches.Count(m => m.Status == "Upcoming"));
    }

    public class AdminMatchStageGroup
    {
        public string Stage { get; set; } = "";
        public List<AdminMatchDetail> Matches { get; set; } = new();
    }

    public class AdminMatchDetail
    {
        public int      MatchId   { get; set; }
        public DateTime MatchDate { get; set; }
        public string   Stage     { get; set; } = "";
        public string   Status    { get; set; } = "";

        public int     HomeTeamId   { get; set; }
        public string  HomeTeamName { get; set; } = "";
        public string? HomeTeamLogo { get; set; }

        public int     AwayTeamId   { get; set; }
        public string  AwayTeamName { get; set; } = "";
        public string? AwayTeamLogo { get; set; }

        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }

        // Pre-computed by controller — no logic needed in view
        public string StatusBadgeClass { get; set; } = "abadge-upcoming";
        public string CardAccentClass  { get; set; } = "";

        public bool ScoreAvailable => HomeScore.HasValue && AwayScore.HasValue;

        // Logo placeholder initials
        public string HomePlaceholder => HomeTeamName.Length >= 2
            ? HomeTeamName.Substring(0, 2).ToUpper() : HomeTeamName.ToUpper();
        public string AwayPlaceholder => AwayTeamName.Length >= 2
            ? AwayTeamName.Substring(0, 2).ToUpper() : AwayTeamName.ToUpper();

        public List<AdminGoalDetail> Goals { get; set; } = new();
    }

    public class AdminGoalDetail
    {
        public string ScorerName { get; set; } = "";
        public int    Minute     { get; set; }
        public int    TeamId     { get; set; }
        public bool   IsPenalty  { get; set; }
        // Pre-computed label so view doesn't need to compare IDs
        public string TeamName   { get; set; } = "";
    }
}
