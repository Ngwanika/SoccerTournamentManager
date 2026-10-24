using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public sealed class TeamMessagesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamMessagesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var messages = await _dbContext.TeamContactMessages
                .AsNoTracking()
                .Include(message => message.Team)
                .OrderBy(message => message.IsRead)
                .ThenByDescending(message => message.SentAtUtc)
                .ToListAsync(cancellationToken);
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
        {
            var message = await _dbContext.TeamContactMessages
                .SingleOrDefaultAsync(item => item.TeamContactMessageId == id, cancellationToken);
            if (message == null)
                return NotFound();
            message.IsRead = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
