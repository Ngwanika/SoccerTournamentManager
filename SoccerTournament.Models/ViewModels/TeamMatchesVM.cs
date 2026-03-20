// SoccerTournament.Models/Models/ViewModels/TeamMatchesVM.cs

namespace SoccerTournament.Models.ViewModels
{
    public class TeamMatchesVM
    {
        public int    TeamId   { get; set; }
        public string TeamName { get; set; } = "";

        public List<TournamentPickerItem> ApprovedTournaments { get; set; } = new();

        public int?    SelectedTournamentId   { get; set; }
        public string? SelectedTournamentName { get; set; }

        // Pre-grouped and ordered by the controller — view just iterates
        public List<MatchStageGroup> GroupedMatches { get; set; } = new();

        // Total flat count for the header subtitle
        public int TotalMatchCount => GroupedMatches.Sum(g => g.Matches.Count);
    }

    // ── One stage bucket (e.g. "Semi-Final") ─────────────────────────────────
    public class MatchStageGroup
    {
        public string            Stage   { get; set; } = "";
        public List<TeamMatchDetail> Matches { get; set; } = new();
    }

    // ── Sidebar tournament item ───────────────────────────────────────────────
    public class TournamentPickerItem
    {
        public int       TournamentId { get; set; }
        public string    Name         { get; set; } = "";
        public string    Status       { get; set; } = "";
        public DateTime? StartDate    { get; set; }

        // Pre-computed CSS dot class — controller sets this
        public string DotClass { get; set; } = "dot-upcoming";
    }

    // ── Single match detail ───────────────────────────────────────────────────
    public class TeamMatchDetail
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

        // Win | Loss | Draw | Live | Upcoming  — set by controller
        public string Result { get; set; } = "Upcoming";

        // Pre-computed CSS classes — set by controller, no switch needed in view
        public string ResultCardClass { get; set; } = "";
        public string ResultBadgeClass { get; set; } = "badge-upcoming";

        public bool ScoreAvailable => HomeScore.HasValue && AwayScore.HasValue;

        public List<GoalDetail> Goals { get; set; } = new();

        // Helper: is this team playing at home?
        public bool IsHomeTeam(int teamId) => HomeTeamId == teamId;

        // Helper: first two letters for logo placeholder
        public string HomePlaceholder => HomeTeamName.Length >= 2
            ? HomeTeamName.Substring(0, 2).ToUpper() : HomeTeamName.ToUpper();
        public string AwayPlaceholder => AwayTeamName.Length >= 2
            ? AwayTeamName.Substring(0, 2).ToUpper() : AwayTeamName.ToUpper();
    }

    // ── Goal scorer detail ────────────────────────────────────────────────────
    public class GoalDetail
    {
        public string ScorerName { get; set; } = "";
        public int    Minute     { get; set; }
        public int    TeamId     { get; set; }
        public bool   IsPenalty  { get; set; }
    }
}
