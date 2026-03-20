using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.IdentityModel.Tokens;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;



namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PlayerController : Controller
    {        

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PlayerController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int? id)
        {

            if (id is null)
                return NotFound(); // or redirect as appropriate
            
            var players = _unitOfWork.Player.GetAll(p => p.TeamId == id).ToList();
            ViewBag.TeamId = id;
            return View(players);
        }

        public IActionResult Upsert(int? id,int? playerId)
        {
            PlayerVM playerVM = new PlayerVM()
            {
                Player = new Player(),
                TeamList = _unitOfWork.Team.GetAll().Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = u.TeamName,
                    Value = u.TeamId.ToString()
                })
            };

            if (playerId == null || playerId == 0)
            {
                //create
                //playerVM.Player.PlayerId = (int)id;
                playerVM.Player.TeamId = (int)id;
                return View(playerVM);
            }
            else
            {
                //update
                playerVM.Player = _unitOfWork.Player.Get(u => u.PlayerId == playerId);
                return View(playerVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(PlayerVM playerVM, IFormFile? file)
        {
            var obj = playerVM.Player;

            if (obj != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string teamPath = Path.Combine(wwwRootPath, @"images\team");

                    if (!string.IsNullOrEmpty(obj.PlayerPhotoUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.PlayerPhotoUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(teamPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.PlayerPhotoUrl = @"\images\team\" + fileName;
                }

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
                //return RedirectToAction("Index");
                return RedirectToAction("Index", new { id = obj.TeamId });
            }

            return View(playerVM);
        }
     

    }
}
