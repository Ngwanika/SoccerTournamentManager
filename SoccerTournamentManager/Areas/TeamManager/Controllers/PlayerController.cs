using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{

    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamManager + "," + SD.Role_TeamManagerLegacy)]
    public class PlayerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PlayerController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int? id)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null || id != teamId)
                return NotFound();

            var players = _unitOfWork.Player.GetAll(player => player.TeamId == teamId).ToList();
            ViewBag.TeamId = teamId;
            return View(players);
        }

        public async Task<IActionResult> Upsert(int? id, int? playerId)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null || (playerId == null && id != teamId))
                return NotFound();

            PlayerVM playerVM = new PlayerVM()
            {
                Player = new Player(),
                TeamList = _unitOfWork.Team.GetAll(team => team.TeamId == teamId).Select(u => new SelectListItem
                {
                    Text = u.TeamName,
                    Value = u.TeamId.ToString()
                })
            };

            if (playerId == null || playerId == 0)
            {
                playerVM.Player.TeamId = teamId.Value;
                return View(playerVM);
            }

            playerVM.Player = _unitOfWork.Player.Get(player => player.PlayerId == playerId && player.TeamId == teamId);
            return playerVM.Player == null ? NotFound() : View(playerVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(PlayerVM playerVM, IFormFile? file)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null)
                return NotFound();

            var obj = playerVM.Player;

            if (obj != null)
            {
                if (!ModelState.IsValid)
                {
                    playerVM.TeamList = _unitOfWork.Team.GetAll(team => team.TeamId == teamId)
                        .Select(team => new SelectListItem
                        {
                            Text = team.TeamName,
                            Value = team.TeamId.ToString()
                        });
                    return View(playerVM);
                }

                Player? existingPlayer = null;
                if (obj.PlayerId != 0)
                {
                    existingPlayer = _unitOfWork.Player.Get(
                        player => player.PlayerId == obj.PlayerId && player.TeamId == teamId);
                    if (existingPlayer == null)
                        return NotFound();
                }

                var wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var teamPath = Path.Combine(wwwRootPath, "images", "team");

                    if (!string.IsNullOrEmpty(existingPlayer?.PlayerPhotoUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, existingPlayer.PlayerPhotoUrl.TrimStart('\\', '/'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    Directory.CreateDirectory(teamPath);
                    await using (var fileStream = new FileStream(Path.Combine(teamPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    obj.PlayerPhotoUrl = $"/images/team/{fileName}";
                }

                obj.TeamId = teamId.Value;
                if (obj.PlayerId == 0)
                {
                    _unitOfWork.Player.Add(obj);
                    TempData["success"] = "Player created successfully";
                }
                else
                {
                    _unitOfWork.Player.Update(obj);
                    TempData["success"] = "Player updated successfully";
                }

                _unitOfWork.Save();
                return RedirectToAction("Index", new { id = obj.TeamId });
            }

            return BadRequest();
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll(int? id)
        {
            var teamId = await GetAssignedTeamIdAsync();
            if (teamId == null || id != teamId)
                return NotFound();

            List<Player> objplayers = _unitOfWork.Player.GetAll(player => player.TeamId == teamId).ToList();
            return Json(new { data = objplayers });
        }

        #endregion

        private async Task<int?> GetAssignedTeamIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.TeamId is > 0 ? user.TeamId : null;
        }
    }
}
