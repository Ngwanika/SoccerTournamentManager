using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TeamController : Controller
    {        

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TeamController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Team> objTeams = _unitOfWork.Team.GetAll().ToList();

            return View(objTeams);
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
                    if(!string.IsNullOrEmpty(obj.LogoUrl))
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
                  


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Team? categoryFromDb = _unitOfWork.Team.Get(u => u.TeamId == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Team? obj = _unitOfWork.Team.Get(u => u.TeamId == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Team.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Team deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult TeamDetails(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Team? TeamFromDb = _unitOfWork.Team.Get(u => u.TeamId == id);


            if (TeamFromDb == null)
            {
                return NotFound();
            }
            return View(TeamFromDb);
        }

        public IActionResult AvailableTournaments(int teamId)
        {
            // Guard – make sure the team actually exists
            Team? team = _unitOfWork.Team.Get(u => u.TeamId == teamId);
            if (team == null)
                return NotFound();

            // All upcoming tournaments
            var tournaments = _unitOfWork.Tournament
                .GetAll(u => u.Status == "Upcoming");

            // All registrations that belong to this team (any tournament)
            var registrations = _unitOfWork.TournamentRegistration
                .GetAll(r => r.TeamId == teamId);

            // Build the view model
            var viewModel = tournaments.Select(t => new TournamentStatusVM
            {
                TournamentId = t.TournamentId,
                TournamentName = t.Name,
                Deadline = t.DeadlineDate,
                // null when the team has never applied to this tournament
                Status = registrations
                                    .FirstOrDefault(r => r.TournamentId == t.TournamentId)
                                    ?.Status
            }).ToList();

            // Pass the team name to the view so the heading is descriptive
            ViewBag.TeamId = teamId;
            ViewBag.TeamName = team.TeamName;

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult ApplyToTournament(int teamId, int tournamentId)
        {
            // Check if already applied (avoid duplicates)
            var existing = _unitOfWork.TournamentRegistration
                .Get(r => r.TeamId == teamId && r.TournamentId == tournamentId);

            if (existing == null)
            {
                _unitOfWork.TournamentRegistration.Add(new TournamentRegistration
                {
                    TeamId = teamId,
                    TournamentId = tournamentId,
                    Status = "Pending",
                    AppliedDate = DateTime.Now
                });
                _unitOfWork.Save();
                TempData["success"] = "Application submitted! Awaiting admin approval.";
            }
            else if (existing.Status == "Rejected")
            {
                // Allow re-apply after rejection
                existing.Status = "Pending";
                existing.AppliedDate = DateTime.Now;
                existing.ReviewedDate = null;
                existing.AdminNotes = null;
                _unitOfWork.TournamentRegistration.Update(existing);
                _unitOfWork.Save();
                TempData["success"] = "Re-application submitted!";
            }
            else
            {
                TempData["error"] = "You have already applied to this tournament.";
            }

            return RedirectToAction("AvailableTournaments", new { teamId });
        }
        // Add to your existing TeamController.
        // Required using: using SoccerTournament.Models.Models.ViewModels;



    }
}
