using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;
using SoccerTournamentManager.Services;

namespace SoccerTournamentManager.Areas.MatchOfficials
{
    [Area("MatchOfficials")]
    [Authorize(Roles = SD.Role_MatchOfficial + "," + SD.Role_Admin + "," + SD.Role_TournamentManager)]
    public sealed class MatchesController : Controller
    {
        private static readonly string[] EventTypes =
        [
            "Goal", "Yellow card", "Red card", "Corner", "Penalty", "Offside", "Substitution", "Other"
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TournamentBracketService _bracketService;

        public MatchesController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            TournamentBracketService bracketService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _bracketService = bracketService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var isManager = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_TournamentManager);
            var userId = _userManager.GetUserId(User);
            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Include(match => match.HomeTeam)
                .Include(match => match.AwayTeam)
                .Include(match => match.Tournament)
                .Where(match => isManager || match.AssignedOfficialUserId == userId
                    || match.AssistantOfficialUserId == userId)
                .OrderBy(match => match.MatchDate)
                .ToListAsync(cancellationToken);
            return View(matches);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id, CancellationToken cancellationToken)
        {
            var match = await LoadMatchAsync(id, cancellationToken);
            if (match == null)
                return NotFound();
            if (!await CanManageAsync(match))
                return Forbid();

            var model = new MatchOfficialWorkspaceViewModel
            {
                Match = match,
                TournamentName = match.Tournament?.Name ?? "Tournament",
                Players = await _dbContext.Players.AsNoTracking()
                    .Where(player => player.TeamId == match.HomeTeamId || player.TeamId == match.AwayTeamId)
                    .OrderBy(player => player.TeamId)
                    .ThenBy(player => player.JerseyNumber)
                    .ToListAsync(cancellationToken),
                Events = await _dbContext.MatchEvents.AsNoTracking()
                    .Include(item => item.Team)
                    .Include(item => item.Player)
                    .Where(item => item.MatchId == id)
                    .OrderByDescending(item => item.EventMinute)
                    .ThenByDescending(item => item.RecordedAtUtc)
                    .ToListAsync(cancellationToken),
                AuditHistory = await _dbContext.MatchAuditLogs.AsNoTracking()
                    .Where(item => item.MatchId == id)
                    .OrderByDescending(item => item.CreatedAtUtc)
                    .Take(50)
                    .ToListAsync(cancellationToken)
            };
            ViewBag.EventTypes = EventTypes;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, CancellationToken cancellationToken)
        {
            var match = await LoadMatchAsync(id, cancellationToken);
            if (match == null)
                return NotFound();
            if (!await CanManageAsync(match))
                return Forbid();

            var normalizedStatus = status.Trim();
            if (normalizedStatus == "Live" && match.Status is not ("Upcoming" or "Scheduled"))
                return BadRequest("Only an upcoming match can be started.");
            if (normalizedStatus == "Completed" && match.Status != "Live")
                return BadRequest("Only a live match can be completed.");
            if (normalizedStatus == "Completed"
                && (!match.HomeScore.HasValue || !match.AwayScore.HasValue))
            {
                match.HomeScore ??= 0;
                match.AwayScore ??= 0;
            }
            if (normalizedStatus is not ("Live" or "Completed"))
                return BadRequest("Unsupported match status.");

            var previousStatus = match.Status;
            match.Status = normalizedStatus;
            AddAudit(match.MatchId, "Status changed", $"{previousStatus ?? "Unspecified"} → {normalizedStatus}");
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (normalizedStatus == "Completed")
                await _bracketService.AdvanceKnockoutAsync(match.TournamentId, cancellationToken);

            TempData["success"] = normalizedStatus == "Live" ? "Match started." : "Match completed.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordEvent(int id, MatchEventRequest request, CancellationToken cancellationToken)
        {
            var match = await LoadMatchAsync(id, cancellationToken);
            if (match == null)
                return NotFound();
            if (!await CanManageAsync(match))
                return Forbid();
            if (!string.Equals(match.Status, "Live", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("", "Start the match before recording an event.");
            if (!EventTypes.Contains(request.EventType, StringComparer.Ordinal))
                ModelState.AddModelError(nameof(request.EventType), "Choose a valid match event.");
            if (request.TeamId != match.HomeTeamId && request.TeamId != match.AwayTeamId)
                ModelState.AddModelError(nameof(request.TeamId), "Choose one of the teams playing this match.");
            if (request.PlayerId.HasValue && !await _dbContext.Players.AnyAsync(player =>
                    player.PlayerId == request.PlayerId && player.TeamId == request.TeamId, cancellationToken))
                ModelState.AddModelError(nameof(request.PlayerId), "The selected player is not on that team.");

            if (!ModelState.IsValid)
            {
                TempData["error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                return RedirectToAction(nameof(Manage), new { id });
            }

            var player = request.PlayerId.HasValue
                ? await _dbContext.Players.SingleAsync(item => item.PlayerId == request.PlayerId, cancellationToken)
                : null;
            var actor = await _userManager.GetUserAsync(User);
            var matchEvent = new MatchEvent
            {
                MatchId = id,
                TeamId = request.TeamId,
                PlayerId = request.PlayerId,
                EventType = request.EventType,
                EventMinute = request.EventMinute,
                Notes = request.Notes?.Trim(),
                RecordedByUserId = actor?.Id,
                RecordedAtUtc = DateTime.UtcNow
            };
            _dbContext.MatchEvents.Add(matchEvent);
            if (request.EventType == "Goal")
            {
                if (request.TeamId == match.HomeTeamId)
                    match.HomeScore = (match.HomeScore ?? 0) + 1;
                else
                    match.AwayScore = (match.AwayScore ?? 0) + 1;

                _dbContext.Goals.Add(new Goal
                {
                    MatchId = id,
                    TeamId = request.TeamId,
                    ScorerName = player == null ? "Unassigned scorer" : $"{player.FirstName} {player.LastName}",
                    Minute = Math.Clamp(request.EventMinute, 1, 120),
                    IsPenalty = request.Notes?.Contains("penalty", StringComparison.OrdinalIgnoreCase) == true
                });
            }

            AddAudit(id, "Event recorded",
                $"{request.EventMinute}' {request.EventType} for team {request.TeamId}"
                + (player == null ? "" : $" ({player.FirstName} {player.LastName})"));
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["success"] = "Match event recorded.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        private async Task<Match?> LoadMatchAsync(int id, CancellationToken cancellationToken) =>
            await _dbContext.Matches
                .Include(match => match.HomeTeam)
                .Include(match => match.AwayTeam)
                .Include(match => match.Tournament)
                .SingleOrDefaultAsync(match => match.MatchId == id, cancellationToken);

        private async Task<bool> CanManageAsync(Match match)
        {
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_TournamentManager))
                return true;
            var userId = _userManager.GetUserId(User);
            return userId != null
                && (match.AssignedOfficialUserId == userId || match.AssistantOfficialUserId == userId);
        }

        private void AddAudit(int matchId, string action, string details)
        {
            _dbContext.MatchAuditLogs.Add(new MatchAuditLog
            {
                MatchId = matchId,
                Action = action,
                Details = details,
                ActorUserId = _userManager.GetUserId(User),
                ActorName = User.Identity?.Name,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }
}
