using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{
    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamManager + "," + SD.Role_TeamManagerLegacy)]
    public sealed class CommunicationController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailSender _emailSender;
        private readonly ILogger<CommunicationController> _logger;

        public CommunicationController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            EmailSender emailSender,
            ILogger<CommunicationController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var team = await GetTeamAsync(cancellationToken);
            if (team == null)
                return NotFound("No team is assigned to this account.");

            var model = new TeamCommunicationViewModel
            {
                WhatsAppGroupLink = team.WhatsAppGroupLink,
                Members = await _dbContext.Players.AsNoTracking()
                    .Where(player => player.TeamId == team.TeamId)
                    .OrderBy(player => player.LastName)
                    .ThenBy(player => player.FirstName)
                    .Select(player => new TeamMemberEmailViewModel
                    {
                        PlayerId = player.PlayerId,
                        Name = player.FirstName + " " + player.LastName,
                        Email = player.Email
                    })
                    .ToListAsync(cancellationToken)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWhatsAppLink(string? whatsAppGroupLink, CancellationToken cancellationToken)
        {
            var team = await GetTeamAsync(cancellationToken);
            if (team == null)
                return NotFound();

            var candidate = whatsAppGroupLink?.Trim();
            if (!string.IsNullOrEmpty(candidate)
                && (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
                    || uri.Scheme != Uri.UriSchemeHttps
                    || !uri.Host.Equals("chat.whatsapp.com", StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(TeamCommunicationViewModel.WhatsAppGroupLink),
                    "Enter a WhatsApp invite URL from https://chat.whatsapp.com/.");
                var members = await _dbContext.Players.AsNoTracking()
                    .Where(player => player.TeamId == team.TeamId)
                    .OrderBy(player => player.LastName)
                    .ThenBy(player => player.FirstName)
                    .Select(player => new TeamMemberEmailViewModel
                    {
                        PlayerId = player.PlayerId,
                        Name = player.FirstName + " " + player.LastName,
                        Email = player.Email
                    })
                    .ToListAsync(cancellationToken);
                return View("Index", new TeamCommunicationViewModel
                {
                    WhatsAppGroupLink = team.WhatsAppGroupLink,
                    Members = members
                });
            }

            team.WhatsAppGroupLink = string.IsNullOrWhiteSpace(candidate) ? null : candidate;
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["success"] = "WhatsApp group invite saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailMembers(TeamEmailRequest request, CancellationToken cancellationToken)
        {
            var team = await GetTeamAsync(cancellationToken);
            if (team == null)
                return NotFound();

            var query = _dbContext.Players.AsNoTracking().Where(player => player.TeamId == team.TeamId);
            if (!request.SendToAll)
                query = query.Where(player => player.PlayerId == request.PlayerId);

            var recipients = await query.Where(player => player.Email != null && player.Email != "")
                .Select(player => player.Email!)
                .ToListAsync(cancellationToken);
            if (!request.SendToAll && request.PlayerId == null)
                ModelState.AddModelError(nameof(request.PlayerId), "Choose a team member.");
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Enter a subject and message, then choose a valid recipient.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _emailSender.SendTeamMessageAsync(recipients, request.Subject, request.Message);
                TempData["success"] = $"Email sent to {recipients.Distinct(StringComparer.OrdinalIgnoreCase).Count()} team member(s).";
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(exception, "Team email could not be sent for team {TeamId}.", team.TeamId);
                TempData["error"] = exception.Message;
            }
            catch (System.Net.Mail.SmtpException)
            {
                TempData["error"] = "Email delivery failed. Check the server mail configuration and logs.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Team?> GetTeamAsync(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeamId is not > 0)
                return null;
            return await _dbContext.Teams.SingleOrDefaultAsync(
                team => team.TeamId == user.TeamId.Value, cancellationToken);
        }
    }
}
