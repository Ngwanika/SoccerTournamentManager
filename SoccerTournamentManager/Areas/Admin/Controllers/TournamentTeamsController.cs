using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;
using SoccerTournamentManager.Services;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_TournamentManager)]
    public sealed class TournamentTeamsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TournamentBracketService _bracketService;

        public TournamentTeamsController(
            ApplicationDbContext dbContext,
            TournamentBracketService bracketService)
        {
            _dbContext = dbContext;
            _bracketService = bracketService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int tournamentId)
        {
            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.TournamentId == tournamentId);
            if (tournament == null)
                return NotFound();

            var participatingTeams = await _dbContext.TournamentTeams
                .AsNoTracking()
                .Where(member => member.TournamentId == tournamentId)
                .Join(_dbContext.Teams, member => member.TeamId, team => team.TeamId,
                    (member, team) => new { member, team })
                .Select(item => new TournamentTeamRowViewModel
                {
                    TeamId = item.team.TeamId,
                    TeamName = item.team.TeamName,
                    PlayerCount = _dbContext.Players.Count(player => player.TeamId == item.team.TeamId),
                    JoinedAt = item.member.DateJoined,
                    HasFixtures = _dbContext.Matches.Any(match => match.TournamentId == tournamentId
                        && (match.HomeTeamId == item.team.TeamId || match.AwayTeamId == item.team.TeamId))
                })
                .OrderBy(item => item.TeamName)
                .ToListAsync();

            var participatingIds = participatingTeams.Select(team => team.TeamId).ToArray();
            var availableTeams = await _dbContext.Teams
                .AsNoTracking()
                .Where(team => !participatingIds.Contains(team.TeamId))
                .OrderBy(team => team.TeamName)
                .Select(team => new TournamentTeamRowViewModel
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    PlayerCount = _dbContext.Players.Count(player => player.TeamId == team.TeamId)
                })
                .ToListAsync();

            var model = new TournamentTeamsManageViewModel
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                ParticipatingTeams = participatingTeams,
                AvailableTeams = availableTeams,
                MatchCount = await _dbContext.Matches.CountAsync(match => match.TournamentId == tournamentId),
                DrawFinalized = await _dbContext.TournamentDraws.AnyAsync(draw => draw.TournamentId == tournamentId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeam(
            int tournamentId,
            int teamId,
            CancellationToken cancellationToken)
        {
            if (!await _dbContext.Tournaments.AnyAsync(item => item.TournamentId == tournamentId)
                || !await _dbContext.Teams.AnyAsync(item => item.TeamId == teamId))
                return NotFound();
            if (await IsDrawLockedAsync(tournamentId, cancellationToken))
            {
                TempData["error"] = "Teams cannot be added after the tournament draw or fixtures have been finalized.";
                return RedirectToAction(nameof(Manage), new { tournamentId });
            }

            if (!await _dbContext.TournamentTeams.AnyAsync(item =>
                    item.TournamentId == tournamentId && item.TeamId == teamId))
            {
                _dbContext.TournamentTeams.Add(new TournamentTeam
                {
                    TournamentId = tournamentId,
                    TeamId = teamId,
                    DateJoined = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync();
            }

            TempData["success"] = "Team added to the tournament.";
            return RedirectToAction(nameof(Manage), new { tournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeam(
            int tournamentId,
            int teamId,
            CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TournamentTeams.SingleOrDefaultAsync(item =>
                item.TournamentId == tournamentId && item.TeamId == teamId);
            if (enrollment == null)
                return NotFound();
            if (await IsDrawLockedAsync(tournamentId, cancellationToken))
            {
                TempData["error"] = "Teams cannot be removed after the tournament draw or fixtures have been finalized.";
                return RedirectToAction(nameof(Manage), new { tournamentId });
            }

            var relatedMatches = await _dbContext.Matches
                .Where(match => match.TournamentId == tournamentId
                    && (match.HomeTeamId == teamId || match.AwayTeamId == teamId))
                .ToListAsync();
            if (relatedMatches.Count > 0)
            {
                TempData["error"] = "This team already appears in a fixture. Remove or reschedule its matches before removing the team.";
                return RedirectToAction(nameof(Manage), new { tournamentId });
            }

            var groupMemberships = await _dbContext.GroupTeams
                .Where(item => item.TeamId == teamId
                    && _dbContext.Groups.Any(group => group.GroupId == item.GroupId
                        && group.TournamentId == tournamentId))
                .ToListAsync();
            _dbContext.GroupTeams.RemoveRange(groupMemberships);
            _dbContext.TournamentTeams.Remove(enrollment);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "Team removed from the tournament.";
            return RedirectToAction(nameof(Manage), new { tournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeDraw(
            int tournamentId,
            string? orderedTeamIds,
            bool randomize,
            CancellationToken cancellationToken)
        {
            try
            {
                await _bracketService.FinalizeDrawAsync(
                    tournamentId,
                    randomize ? null : orderedTeamIds,
                    User.Identity?.Name ?? "",
                    cancellationToken);
                TempData["success"] = randomize
                    ? "Random draw saved and fixtures generated."
                    : "Seed order saved and fixtures generated.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                TempData["error"] = exception.Message;
            }

            return RedirectToAction(nameof(Manage), new { tournamentId });
        }

        private async Task<bool> IsDrawLockedAsync(int tournamentId, CancellationToken cancellationToken) =>
            await _dbContext.TournamentDraws.AnyAsync(draw => draw.TournamentId == tournamentId, cancellationToken)
            || await _dbContext.Matches.AnyAsync(match => match.TournamentId == tournamentId, cancellationToken);
    }
}
