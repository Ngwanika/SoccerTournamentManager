using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{

    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamManager + "," + SD.Role_TeamManagerLegacy)]
    public class TournamentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public TournamentsController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AvailableTournaments(int teamId)
        {
            var assignedTeamId = await GetAssignedTeamIdAsync();
            if (assignedTeamId == null)
                return NotFound("No team assigned.");

            Team? team = _unitOfWork.Team.Get(u => u.TeamId == assignedTeamId.Value);
            if (team == null)
                return NotFound();

            var tournaments = _unitOfWork.Tournament
                .GetAll(tournament => tournament.Status == "Upcoming");

            var registrations = _unitOfWork.TournamentRegistration
                .GetAll(registration => registration.TeamId == assignedTeamId.Value);

            var viewModel = tournaments.Select(t => new TournamentStatusVM
            {
                TournamentId = t.TournamentId,
                TournamentName = t.Name,
                Deadline = t.DeadlineDate,
                Status = registrations
                                    .FirstOrDefault(r => r.TournamentId == t.TournamentId)
                                    ?.Status
            }).ToList();

            ViewBag.TeamId = assignedTeamId.Value;
            ViewBag.TeamName = team.TeamName;

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyToTournament(int teamId, int tournamentId)
        {
            var assignedTeamId = await GetAssignedTeamIdAsync();
            if (assignedTeamId == null)
                return NotFound("No team assigned.");

            var tournament = _unitOfWork.Tournament.Get(
                item => item.TournamentId == tournamentId && item.Status == "Upcoming");
            if (tournament == null)
                return NotFound();

            if (tournament.DeadlineDate < DateTime.Today)
            {
                TempData["error"] = "The registration deadline has passed.";
                return RedirectToAction(nameof(AvailableTournaments), new { teamId = assignedTeamId.Value });
            }

            var existing = _unitOfWork.TournamentRegistration
                .Get(r => r.TeamId == assignedTeamId.Value && r.TournamentId == tournamentId);

            if (existing == null)
            {
                _unitOfWork.TournamentRegistration.Add(new TournamentRegistration
                {
                    TeamId = assignedTeamId.Value,
                    TournamentId = tournamentId,
                    Status = "Pending",
                    AppliedDate = DateTime.Now
                });
                _unitOfWork.Save();
                TempData["success"] = "Application submitted! Awaiting admin approval.";
            }
            else if (existing.Status == "Rejected")
            {
                existing.Status = "Pending";
                existing.AppliedDate = DateTime.Now;
                existing.ReviewedDate = null;
                existing.AdminNotes = null;
                _unitOfWork.TournamentRegistration.Update(existing);
                _unitOfWork.Save();
                TempData["success"] = "Re-application submitted!";
            }
            else
            {
                TempData["error"] = "You have already applied to this tournament.";
            }

            return RedirectToAction(nameof(AvailableTournaments), new { teamId = assignedTeamId.Value });
        }

        public async Task<IActionResult> Details(int teamId, int tournamentId)
        {
            var assignedTeamId = await GetAssignedTeamIdAsync();
            if (assignedTeamId == null)
                return NotFound("No team assigned.");

            if (tournamentId <= 0)
            {
                return NotFound();
            }

            var tournament = _unitOfWork.Tournament.Get(u => u.TournamentId == tournamentId);

            if (tournament == null)
            {
                return NotFound();
            }
            ViewBag.TeamId = assignedTeamId.Value;
            return View(tournament);
        }

        public async Task<IActionResult> ViewMatches(int? teamId, int tournamentId)
        {
            var assignedTeamId = await GetAssignedTeamIdAsync();
            if (assignedTeamId == null)
                return NotFound("No team assigned.");

            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == tournamentId);
            if (tournament == null) return NotFound();

            var matches = _unitOfWork.Match.GetAll(
                match => match.TournamentId == tournamentId
                && (match.HomeTeamId == assignedTeamId || match.AwayTeamId == assignedTeamId),
                includeProperties: "HomeTeam,AwayTeam,Goals"
            ).OrderBy(match => match.MatchDate).ToList();
                

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
            string teamName = null;
           

            var team = _unitOfWork.Team.Get(t => t.TeamId == assignedTeamId.Value);
            teamName = team?.TeamName;
            var vm = new TournamentMatchesVM
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped,
                TeamName = teamName,
                TeamId = assignedTeamId.Value
            };
           
            return View(vm);
        }

        public async Task<IActionResult> ViewAllMatches(int? teamId, int tournamentId)
        {
            var assignedTeamId = await GetAssignedTeamIdAsync();
            if (assignedTeamId == null)
                return NotFound("No team assigned.");

            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == tournamentId);
            if (tournament == null) return NotFound();

            var matches = _unitOfWork.Match.GetAll(
                match => match.TournamentId == tournamentId
                    && (match.HomeTeamId == assignedTeamId || match.AwayTeamId == assignedTeamId),
                includeProperties: "HomeTeam,AwayTeam,Goals")
                .OrderBy(match => match.MatchDate)
                .ToList();


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
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped,
                TeamId = assignedTeamId.Value
            };
            return View(vm);
        }

        private async Task<int?> GetAssignedTeamIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.TeamId is > 0 ? user.TeamId : null;
        }
    }
}
