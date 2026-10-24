using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{
    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamManager + "," + SD.Role_TeamManagerLegacy)]

    public class TeamController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeamController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null)
                return NotFound("No team assigned.");

            Team? team = _unitOfWork.Team.Get(u => u.TeamId == teamId.Value);
            if (team == null)
                return NotFound("Team not found.");

            return View(team);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null || id != teamId)
            {
                return NotFound();
            }

            var team = _unitOfWork.Team.Get(u => u.TeamId == teamId.Value);
            return team == null ? NotFound() : View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Team obj, IFormFile? file)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null || obj.TeamId != teamId)
            {
                return NotFound();
            }

            var existingTeam = _unitOfWork.Team.Get(team => team.TeamId == teamId.Value);
            if (existingTeam == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(obj);
            }
            if (!string.IsNullOrWhiteSpace(obj.WhatsAppGroupLink)
                && (!Uri.TryCreate(obj.WhatsAppGroupLink, UriKind.Absolute, out var inviteUri)
                    || inviteUri.Scheme != Uri.UriSchemeHttps
                    || !inviteUri.Host.Equals("chat.whatsapp.com", StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(obj.WhatsAppGroupLink),
                    "Enter a WhatsApp invite URL from https://chat.whatsapp.com/.");
                return View(obj);
            }

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var teamPath = Path.Combine(wwwRootPath, "images", "team");
                if (!string.IsNullOrEmpty(existingTeam.LogoUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, existingTeam.LogoUrl.TrimStart('\\', '/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                Directory.CreateDirectory(teamPath);
                await using (var fileStream = new FileStream(Path.Combine(teamPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                existingTeam.LogoUrl = $"/images/team/{fileName}";
            }

            existingTeam.TeamName = obj.TeamName;
            existingTeam.Abbreviation = obj.Abbreviation;
            existingTeam.CoachName = obj.CoachName;
            existingTeam.Homestadium = obj.Homestadium;
            existingTeam.ContactNumber1 = obj.ContactNumber1;
            existingTeam.ContactNumber2 = obj.ContactNumber2;
            existingTeam.Email = obj.Email;
            existingTeam.WhatsAppGroupLink = obj.WhatsAppGroupLink;
            existingTeam.Established = obj.Established;
            _unitOfWork.Team.Update(existingTeam);
            _unitOfWork.Save();
            TempData["success"] = "Team updated successfully";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetAssignedTeamIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.TeamId is > 0 ? user.TeamId : null;
        }
    }
}
