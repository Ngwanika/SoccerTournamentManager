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
    public sealed class ContactAdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactAdminController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (await GetTeamIdAsync() == null)
                return NotFound("No team is assigned to this account.");
            return View(new TeamContactAdminRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TeamContactAdminRequest request, CancellationToken cancellationToken)
        {
            var teamId = await GetTeamIdAsync();
            var user = await _userManager.GetUserAsync(User);
            if (teamId == null || user == null)
                return NotFound("No team is assigned to this account.");
            if (!ModelState.IsValid)
                return View(request);
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("", "Add a confirmed contact email to your account before contacting the administrator.");
                return View(request);
            }

            _dbContext.TeamContactMessages.Add(new TeamContactMessage
            {
                TeamId = teamId.Value,
                SenderUserId = user.Id,
                ReplyEmail = user.Email,
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                SentAtUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["success"] = "Your message was sent to the tournament administrators.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetTeamIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.TeamId is > 0 ? user.TeamId : null;
        }
    }
}
