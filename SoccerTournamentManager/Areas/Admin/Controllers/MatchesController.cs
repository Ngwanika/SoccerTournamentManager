using Microsoft.AspNetCore.Mvc;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournamentManager.Services;
using SoccerTournament.Models;
using System.Linq;
using System.Collections.Generic;
using SoccerTournamentManager.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SoccerTournament.Models.ViewModels;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class MatchesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MatchGeneratorService _matchGenerator;

        public MatchesController(IUnitOfWork unitOfWork,
                                 MatchGeneratorService matchGenerator)
        {
            _unitOfWork = unitOfWork;
            _matchGenerator = matchGenerator;
        }

        // Generate Matches
        public IActionResult Generate(int tournamentId)
        {
            _matchGenerator.GenerateLeagueMatches(tournamentId);

            TempData["success"] = "Matches generated successfully";

            return RedirectToAction(nameof(Index),
                new { tournamentId });
        }

        // View Matches
        public IActionResult Index(int tournamentId)
        {
            var matches = _unitOfWork.Match
                .GetAll()
                .Where(x => x.TournamentId == tournamentId)
                .ToList();

            // Load all teams
            var teams = _unitOfWork.Team
                .GetAll()
                .ToDictionary(x => x.TeamId, x => x.TeamName);

            ViewBag.Teams = teams;
            ViewBag.TournamentId = tournamentId;

            return View(matches);
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
