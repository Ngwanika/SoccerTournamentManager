namespace SoccerTournament.Models.ViewModels
{
    public sealed class AdminUserRoleViewModel
    {
        public required string UserId { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public required string CurrentRoles { get; init; }
        public required string SelectedRole { get; init; }
        public int? TeamId { get; init; }
        public required IReadOnlyList<string> Roles { get; init; }
        public required IReadOnlyList<AdminTeamOption> Teams { get; init; }
    }

    public sealed class AdminTeamOption
    {
        public int TeamId { get; init; }
        public required string TeamName { get; init; }
    }
}
