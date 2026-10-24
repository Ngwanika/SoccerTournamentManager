using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournamentManager.Services;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_TournamentManager)]
    public class MatchesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TournamentBracketService _bracketService;

        public MatchesController(IUnitOfWork unitOfWork,
                                 ApplicationDbContext dbContext,
                                 UserManager<ApplicationUser> userManager,
                                 TournamentBracketService bracketService)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _userManager = userManager;
            _bracketService = bracketService;
        }

        // Generate Matches
        public async Task<IActionResult> Generate(int tournamentId, CancellationToken cancellationToken)
        {
            try
            {
                await _bracketService.FinalizeDrawAsync(
                    tournamentId,
                    null,
                    _userManager.GetUserId(User) ?? "",
                    cancellationToken);
                TempData["success"] = "Random draw saved and fixtures generated.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                TempData["error"] = exception.Message;
            }

            return RedirectToAction(nameof(Index),
                new { tournamentId });
        }

        // View Matches
        public async Task<IActionResult> Index(int tournamentId, CancellationToken cancellationToken)
        {
            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Where(match => match.TournamentId == tournamentId)
                .OrderBy(match => match.MatchDate)
                .ToListAsync(cancellationToken);

            // Load all teams
            var teams = _unitOfWork.Team
                .GetAll()
                .ToDictionary(x => x.TeamId, x => x.TeamName);

            ViewBag.Teams = teams;
            ViewBag.TournamentId = tournamentId;
            var officials = await _userManager.GetUsersInRoleAsync(SD.Role_MatchOfficial);
            ViewBag.Officials = officials.OrderBy(user => user.Name).ToList();

            return View(matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignOfficials(
            int matchId,
            string? leadOfficialUserId,
            string? assistantOfficialUserId,
            CancellationToken cancellationToken)
        {
            var match = await _dbContext.Matches.SingleOrDefaultAsync(
                item => item.MatchId == matchId, cancellationToken);
            if (match == null)
                return NotFound();

            var officialIds = new[] { leadOfficialUserId, assistantOfficialUserId }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();
            var officials = await _userManager.GetUsersInRoleAsync(SD.Role_MatchOfficial);
            if (officialIds.Any(id => officials.All(official => official.Id != id)))
                return BadRequest("Choose users assigned the Match Official role.");
            if (!string.IsNullOrWhiteSpace(leadOfficialUserId)
                && leadOfficialUserId == assistantOfficialUserId)
                return BadRequest("The lead and assistant officials must be different users.");

            var oldLead = match.AssignedOfficialUserId;
            var oldAssistant = match.AssistantOfficialUserId;
            match.AssignedOfficialUserId = leadOfficialUserId;
            match.AssistantOfficialUserId = assistantOfficialUserId;
            _dbContext.MatchAuditLogs.Add(new MatchAuditLog
            {
                MatchId = matchId,
                Action = "Officials assigned",
                Details = $"Lead: {oldLead ?? "none"} → {leadOfficialUserId ?? "none"}; "
                    + $"Assistant: {oldAssistant ?? "none"} → {assistantOfficialUserId ?? "none"}",
                ActorUserId = _userManager.GetUserId(User),
                ActorName = User.Identity?.Name,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["success"] = "Match officials updated.";
            return RedirectToAction(nameof(Index), new { tournamentId = match.TournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScore(
            int matchId,
            int homeScore,
            int awayScore,
            int? winnerTeamId,
            CancellationToken cancellationToken)
        {
            if (homeScore < 0 || awayScore < 0 || homeScore > 99 || awayScore > 99)
                return BadRequest("Scores must be between 0 and 99.");
            var match = await _dbContext.Matches.SingleOrDefaultAsync(
                item => item.MatchId == matchId, cancellationToken);
            if (match == null)
                return NotFound();
            var laterBracketRoundExists = match.BracketRound.HasValue
                && await _dbContext.Matches.AnyAsync(item =>
                    item.TournamentId == match.TournamentId
                    && item.BracketRound > match.BracketRound, cancellationToken);
            var groupKnockoutStarted = match.Stage?.StartsWith("Group ", StringComparison.OrdinalIgnoreCase) == true
                && await _dbContext.Matches.AnyAsync(item =>
                    item.TournamentId == match.TournamentId && item.BracketRound.HasValue, cancellationToken);
            if (laterBracketRoundExists || groupKnockoutStarted)
            {
                TempData["error"] = "This match result cannot be changed after the next tournament stage has started.";
                return RedirectToAction(nameof(Index), new { tournamentId = match.TournamentId });
            }
            if (match.BracketRound.HasValue && homeScore == awayScore
                && winnerTeamId != match.HomeTeamId && winnerTeamId != match.AwayTeamId)
            {
                TempData["error"] = "Select the winning team when a knockout score is tied.";
                return RedirectToAction(nameof(Index), new { tournamentId = match.TournamentId });
            }

            var previous = $"{match.HomeScore?.ToString() ?? "–"}-{match.AwayScore?.ToString() ?? "–"}";
            match.HomeScore = homeScore;
            match.AwayScore = awayScore;
            match.WinnerTeamId = match.BracketRound.HasValue && homeScore == awayScore
                ? winnerTeamId
                : null;
            match.Status = "Completed";
            _dbContext.MatchAuditLogs.Add(new MatchAuditLog
            {
                MatchId = matchId,
                Action = "Score updated by tournament manager",
                Details = $"{previous} → {homeScore}-{awayScore}",
                ActorUserId = _userManager.GetUserId(User),
                ActorName = User.Identity?.Name,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _bracketService.AdvanceKnockoutAsync(match.TournamentId, cancellationToken);
            TempData["success"] = "Match result saved.";
            return RedirectToAction(nameof(Index), new { tournamentId = match.TournamentId });
        }

        //public IActionResult ViewMatches(int tournamentId)
        //{
        //    var matches = _unitOfWork.Match
        //        .GetAll()
        //        .Where(x => x.TournamentId == tournamentId)
        //        .ToList();

        //    // Load all teams
        //    var teams = _unitOfWork.Team
        //        .GetAll()
        //        .ToDictionary(x => x.TeamId, x => x.TeamName);

        //    ViewBag.Teams = teams;
        //    ViewBag.TournamentId = tournamentId;

        //    return View(matches);
        //}

        public IActionResult ViewMatches(int id)
        {
            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == id);
            if (tournament == null) return NotFound();

            // All matches in this tournament, with team and goal data
            var matches = _unitOfWork.Match.GetAll(
                m => m.TournamentId == id,
                includeProperties: "HomeTeam,AwayTeam,Goals"
            ).OrderBy(m => m.MatchDate).ToList();

            // Stage display order
            var stageOrder = new List<string>
    {
        "Group Stage", "Round of 16", "Quarter-Final", "Semi-Final", "Final"
    };

            // Build detail objects — all CSS decisions made here
            var details = matches.Select(m =>
            {
                string statusBadge = m.Status == "Live" ? "abadge-live"
                                   : m.Status == "Completed" ? "abadge-completed"
                                   : "abadge-upcoming";

                string cardAccent = m.Status == "Live" ? "accent-live"
                                   : m.Status == "Completed" ? "accent-done"
                                   : "accent-upcoming";

                var goals = m.Goals
                    .OrderBy(g => g.Minute)
                    .Select(g => new AdminGoalDetail
                    {
                        ScorerName = g.ScorerName,
                        Minute = g.Minute,
                        TeamId = g.TeamId,
                        IsPenalty = g.IsPenalty,
                        // Resolve team name here so view never compares IDs
                        TeamName = g.TeamId == m.HomeTeamId
                                     ? (m.HomeTeam?.TeamName ?? "")
                                     : (m.AwayTeam?.TeamName ?? "")
                    }).ToList();

                return new AdminMatchDetail
                {
                    MatchId = m.MatchId,
                    MatchDate = m.MatchDate,
                    Stage = m.Stage,
                    Status = m.Status,
                    HomeTeamId = m.HomeTeamId,
                    HomeTeamName = m.HomeTeam?.TeamName ?? "TBD",
                    HomeTeamLogo = m.HomeTeam?.LogoUrl,
                    AwayTeamId = m.AwayTeamId,
                    AwayTeamName = m.AwayTeam?.TeamName ?? "TBD",
                    AwayTeamLogo = m.AwayTeam?.LogoUrl,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    StatusBadgeClass = statusBadge,
                    CardAccentClass = cardAccent,
                    Goals = goals
                };
            }).ToList();

            // Group by stage in defined order
            var grouped = details
                .GroupBy(m => m.Stage)
                .OrderBy(g => stageOrder.Contains(g.Key) ? stageOrder.IndexOf(g.Key) : 99)
                .Select(g => new AdminMatchStageGroup
                {
                    Stage = g.Key,
                    Matches = g.ToList()
                })
                .ToList();

            var vm = new TournamentMatchesVM
            {
                TournamentId = id,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped
            };

            return View(vm);
        }
    }
}
