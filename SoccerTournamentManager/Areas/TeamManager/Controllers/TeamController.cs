using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace SoccerTournamentManager.Areas.TeamManager.Controllers
{
    [Area("TeamManager")]
    [Authorize(Roles = SD.Role_TeamMananger)]

    public class TeamController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;       
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeamController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager, 
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;            
            _userManager = userManager;
             _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var teamId = user.TeamId;

            if (teamId == 0)
                return NotFound("No team assigned.");

            // var team = _unitOfWork.Team.GetFirstOrDefault(u => u.TeamId == teamId);
            Team? team = _unitOfWork.Team.Get(u => u.TeamId == teamId);
            if (team == null)
                return NotFound("Team not found.");

            return View(team);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                Team obj = new Team();
                return View(obj);
            }
            else
            {
                //update
                Team? TeamFromDb = _unitOfWork.Team.Get(u => u.TeamId == id);
                return View(TeamFromDb);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Team obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string teamPath = Path.Combine(wwwRootPath, @"images\team");
                    if (!string.IsNullOrEmpty(obj.LogoUrl))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, obj.LogoUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(teamPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.LogoUrl = @"\images\team\" + fileName;

                }


                if (obj.TeamId == 0)
                {
                    //create
                    _unitOfWork.Team.Add(obj);
                    TempData["success"] = "Team created successfully";
                }
                else
                {
                    //update
                    _unitOfWork.Team.Update(obj);
                    TempData["success"] = "Team updated successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);

        }
    }
}


