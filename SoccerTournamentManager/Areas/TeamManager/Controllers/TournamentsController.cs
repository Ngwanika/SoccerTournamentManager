using Microsoft.AspNetCore.Mvc;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Models;
using SoccerTournament.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using SoccerTournament.Utility;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{

    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamMananger)]
    public class TournamentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TournamentsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AvailableTournaments(int teamId)
        {
            // Guard – make sure the team actually exists
            Team? team = _unitOfWork.Team.Get(u => u.TeamId == teamId);
            if (team == null)
                return NotFound();

            // All upcoming tournaments
            //var tournaments = _unitOfWork.Tournament
            //    .GetAll(u => u.Status == "Upcoming");
            var tournaments = _unitOfWork.Tournament
             .GetAll();

            // All registrations that belong to this team (any tournament)
            var registrations = _unitOfWork.TournamentRegistration
                .GetAll(r => r.TeamId == teamId);

            // Build the view model
            var viewModel = tournaments.Select(t => new TournamentStatusVM
            {
                TournamentId = t.TournamentId,
                TournamentName = t.Name,
                Deadline = t.DeadlineDate,
                // null when the team has never applied to this tournament
                Status = registrations
                                    .FirstOrDefault(r => r.TournamentId == t.TournamentId)
                                    ?.Status
            }).ToList();

            // Pass the team name to the view so the heading is descriptive
            ViewBag.TeamId = teamId;
            ViewBag.TeamName = team.TeamName;

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult ApplyToTournament(int teamId, int tournamentId)
        {
            // Check if already applied (avoid duplicates)
            var existing = _unitOfWork.TournamentRegistration
                .Get(r => r.TeamId == teamId && r.TournamentId == tournamentId);

            if (existing == null)
            {
                _unitOfWork.TournamentRegistration.Add(new TournamentRegistration
                {
                    TeamId = teamId,
                    TournamentId = tournamentId,
                    Status = "Pending",
                    AppliedDate = DateTime.Now
                });
                _unitOfWork.Save();
                TempData["success"] = "Application submitted! Awaiting admin approval.";
            }
            else if (existing.Status == "Rejected")
            {
                // Allow re-apply after rejection
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

            return RedirectToAction("AvailableTournaments", new { teamId });
        }

        public IActionResult Details(int teamId, int tournamentId)
        {
            if (tournamentId == null || tournamentId == 0)
            {
                return NotFound();
            }

            var tournament = _unitOfWork.Tournament.Get(u => u.TournamentId == tournamentId);

            if (tournament == null)
            {
                return NotFound();
            }
            ViewBag.TeamId = teamId;
            return View(tournament);
        }

        public IActionResult ViewMatches(int? teamId, int tournamentId)
        {
            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == tournamentId);
            if (tournament == null) return NotFound();

            // All matches in this tournament, with team and goal data
            var matches = new List<Match>();
            if (teamId.HasValue)
            {
                 matches = _unitOfWork.Match.GetAll(
                m => m.TournamentId == tournamentId
                && (m.HomeTeamId == teamId || m.AwayTeamId == teamId),
                includeProperties: "HomeTeam,AwayTeam,Goals"

            ).OrderBy(m => m.MatchDate).ToList();
            }
            else
            {
                 matches = _unitOfWork.Match.GetAll(
                m => m.TournamentId == tournamentId,
                includeProperties: "HomeTeam,AwayTeam,Goals"
            ).OrderBy(m => m.MatchDate).ToList();
            }
                

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
           

            if (teamId.HasValue)
            {
                var team = _unitOfWork.Team.Get(t => t.TeamId == teamId.Value);
                teamName = team?.TeamName;
            }
            var vm = new TournamentMatchesVM
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped,
                TeamName = teamName,
                TeamId = teamId
            };
           
            return View(vm);
        }

        public IActionResult ViewAllMatches(int? teamId, int tournamentId)
        {
            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == tournamentId);
            if (tournament == null) return NotFound();

            // All matches in this tournament, with team and goal data
            var matches = new List<Match>();
            if (teamId.HasValue)
            {
                matches = _unitOfWork.Match.GetAll(
               m => m.TournamentId == tournamentId,
               includeProperties: "HomeTeam,AwayTeam,Goals"

           ).OrderBy(m => m.MatchDate).ToList();
            }
            else
            {
                matches = _unitOfWork.Match.GetAll(
               m => m.TournamentId == tournamentId,
               includeProperties: "HomeTeam,AwayTeam,Goals"
           ).OrderBy(m => m.MatchDate).ToList();
            }


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


            var vm = new TournamentMatchesVM
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped,                        
            };
            ViewBag.TeamIdN = teamId;
            return View(vm);
        }
    }
}
