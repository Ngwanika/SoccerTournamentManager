using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournamentManager.Services;


namespace SoccerTournamentManager.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;
        private readonly TournamentBracketService _bracketService;

        public HomeController(
            IUnitOfWork unitOfWork,
            ApplicationDbContext dbContext,
            TournamentBracketService bracketService)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _bracketService = bracketService;
        }

        public IActionResult Index()
        {
            // List<Team> objTeams = _unitOfWork.Team.GetAll().ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ViewTornaments()
        {
            var tournamentList = _unitOfWork.Tournament.GetAll();
            return View(tournamentList);

        }

        public IActionResult ViewDetails(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var tournament = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);

        }

        public IActionResult ViewParticipants(int? tournamentId)
        {
            // tournamentId = 2;
            var tournament = _unitOfWork.Tournament
              .Get(x => x.TournamentId == tournamentId);

            if (tournament == null)
                return NotFound();

            // Teams already added
            var addedTeamIds = _unitOfWork.TournamentTeams
                .GetAll()
                .Where(x => x.TournamentId == tournamentId)
                .Select(x => x.TeamId)
                .ToList();

            var addedTeams = _unitOfWork.Team
                .GetAll()
                .Where(x => addedTeamIds.Contains(x.TeamId))
                .ToList();

            // Teams not yet added
            var availableTeams = _unitOfWork.Team
                .GetAll()
                .Where(x => !addedTeamIds.Contains(x.TeamId))
                .ToList();

            TournamentTeamVM vm = new TournamentTeamVM
            {
                TournamentId = (int)tournamentId,
                Tournament = tournament,
                AvailableTeams = availableTeams,
                AddedTeams = addedTeams
            };

            return View(vm);
        }

   
        public IActionResult ViewMatches2(int id)
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

            // Build detail objects � all CSS decisions made here
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

        public IActionResult ViewMatches(int id)
        {
            var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == id);
            if (tournament == null) return NotFound();

            // Get raw matches
            var matches = _unitOfWork.Match.GetAll(
                m => m.TournamentId == id,
                includeProperties: "HomeTeam,AwayTeam,Goals"
            ).OrderBy(m => m.MatchDate).ToList();

            // MAP to AdminMatchDetail (exact same logic as ViewMatches)
            var details = matches.Select(m =>
            {
                string statusBadge = m.Status == "Live" ? "badge bg-success-subtle text-success"
                                 : m.Status == "Completed" ? "badge bg-secondary-subtle text-secondary"
                                 : "badge bg-info-subtle text-info";  // Upcoming

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
                    StatusBadgeClass = statusBadge,  // Full Bootstrap class
                    CardAccentClass = cardAccent,
                    Goals = goals
                };
            }).OrderBy(m => m.MatchDate.Date)
             .ThenBy(m => m.MatchDate.TimeOfDay)
             .ToList();

            // Group by date/stage for schedule view
            var stageOrder = new List<string> { "Group Stage", "Round of 16", "Quarter-Final", "Semi-Final", "Final" };
            var grouped = details
                .GroupBy(m => new { m.MatchDate.Date, m.Stage })
                .OrderBy(g => g.Key.Date)
                .ThenBy(g => stageOrder.IndexOf(g.Key.Stage))
                .Select(g => new AdminMatchStageGroup
                {
                    Stage = $"{g.Key.Date:MMM dd} - {g.Key.Stage}",
                    Matches = g.OrderBy(x => x.MatchDate.TimeOfDay).ToList()
                }).ToList();

            var vm = new TournamentMatchesVM
            {
                TournamentId = id,
                TournamentName = tournament.Name,
                TournamentStatus = tournament.Status,
                GroupedMatches = grouped
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MatchDetails(int id, CancellationToken cancellationToken)
        {
            var match = await _dbContext.Matches
                .AsNoTracking()
                .Include(item => item.HomeTeam)
                .Include(item => item.AwayTeam)
                .Include(item => item.Events)
                    .ThenInclude(item => item.Player)
                .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);
            if (match == null)
                return NotFound();

            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.TournamentId == match.TournamentId, cancellationToken);
            if (tournament == null)
                return NotFound();

            var events = match.Events.OrderBy(item => item.EventMinute).ThenBy(item => item.RecordedAtUtc).ToList();
            var tracked = events.Count > 0;
            var meetings = await _dbContext.Matches
                .AsNoTracking()
                .Where(item => item.MatchId != match.MatchId
                    && item.Status == "Completed"
                    && ((item.HomeTeamId == match.HomeTeamId && item.AwayTeamId == match.AwayTeamId)
                        || (item.HomeTeamId == match.AwayTeamId && item.AwayTeamId == match.HomeTeamId)))
                .Select(item => new { item.HomeTeamId, item.HomeScore, item.AwayScore })
                .ToListAsync(cancellationToken);

            var officialIds = new[] { match.AssignedOfficialUserId, match.AssistantOfficialUserId }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();
            var officials = await _dbContext.Users
                .AsNoTracking()
                .Where(user => officialIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.Name, cancellationToken);

            var players = await _dbContext.Players
                .AsNoTracking()
                .Where(player => player.TeamId == match.HomeTeamId || player.TeamId == match.AwayTeamId)
                .OrderBy(player => player.JerseyNumber)
                .Select(player => new MatchPlayerViewModel
                {
                    PlayerId = player.PlayerId,
                    TeamId = player.TeamId,
                    Name = player.FirstName + " " + player.LastName,
                    JerseyNumber = player.JerseyNumber,
                    Position = player.Position
                })
                .ToListAsync(cancellationToken);

            var model = new MatchDetailsViewModel
            {
                MatchId = match.MatchId,
                TournamentId = match.TournamentId,
                TournamentName = tournament.Name,
                Status = match.Status ?? "Scheduled",
                Stage = match.Stage,
                MatchDate = match.MatchDate,
                HomeTeamId = match.HomeTeamId,
                HomeTeamName = match.HomeTeam?.TeamName ?? "Home team",
                AwayTeamId = match.AwayTeamId,
                AwayTeamName = match.AwayTeam?.TeamName ?? "Away team",
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                LeadOfficialName = match.AssignedOfficialUserId != null
                    ? officials.GetValueOrDefault(match.AssignedOfficialUserId)
                    : null,
                AssistantOfficialName = match.AssistantOfficialUserId != null
                    ? officials.GetValueOrDefault(match.AssistantOfficialUserId)
                    : null,
                HasTrackedEvents = tracked,
                HomeCorners = tracked ? events.Count(item => item.TeamId == match.HomeTeamId && item.EventType == "Corner") : null,
                AwayCorners = tracked ? events.Count(item => item.TeamId == match.AwayTeamId && item.EventType == "Corner") : null,
                HomeOffsides = tracked ? events.Count(item => item.TeamId == match.HomeTeamId && item.EventType == "Offside") : null,
                AwayOffsides = tracked ? events.Count(item => item.TeamId == match.AwayTeamId && item.EventType == "Offside") : null,
                HomePenalties = tracked ? events.Count(item => item.TeamId == match.HomeTeamId && item.EventType == "Penalty") : null,
                AwayPenalties = tracked ? events.Count(item => item.TeamId == match.AwayTeamId && item.EventType == "Penalty") : null,
                PreviousMeetings = meetings.Count,
                HomeWins = meetings.Count(item => item.HomeScore.HasValue && item.AwayScore.HasValue
                    && (item.HomeTeamId == match.HomeTeamId ? item.HomeScore > item.AwayScore : item.AwayScore > item.HomeScore)),
                AwayWins = meetings.Count(item => item.HomeScore.HasValue && item.AwayScore.HasValue
                    && (item.HomeTeamId == match.HomeTeamId ? item.AwayScore > item.HomeScore : item.HomeScore > item.AwayScore)),
                Draws = meetings.Count(item => item.HomeScore.HasValue && item.HomeScore == item.AwayScore),
                HomePlayers = players.Where(player => player.TeamId == match.HomeTeamId).ToList(),
                AwayPlayers = players.Where(player => player.TeamId == match.AwayTeamId).ToList(),
                Events = events.Select(item => new MatchEventViewModel
                {
                    EventType = item.EventType,
                    TeamName = item.TeamId == match.HomeTeamId
                        ? match.HomeTeam?.TeamName ?? "Home team"
                        : match.AwayTeam?.TeamName ?? "Away team",
                    PlayerName = item.Player == null ? null : $"{item.Player.FirstName} {item.Player.LastName}",
                    Minute = item.EventMinute,
                    Notes = item.Notes
                }).ToList()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Brackets(int id, CancellationToken cancellationToken)
        {
            var model = await _bracketService.GetBracketAsync(id, cancellationToken);
            return model == null ? NotFound() : View(model);
        }


    }
}
