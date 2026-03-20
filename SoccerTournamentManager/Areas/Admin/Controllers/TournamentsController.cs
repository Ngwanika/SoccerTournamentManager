using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentsController : Controller
    {
        
        private readonly IUnitOfWork _unitOfWork;
        public TournamentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ======================
        // INDEX
        // ======================
        public IActionResult Index()
        {
            var tournamentList = _unitOfWork.Tournament.GetAll();
            return View(tournamentList);

            //List<Tournament> objTournament = _unitOfWork.Tournament.GetAll().ToList();

            //return View(objTournament);
        }
     
        public IActionResult Upsert(int? id)
        {
            // 1. Populate Format Dropdown from Database
            ViewBag.FormatList = _unitOfWork.TournamentFormat.GetAll().Select(u => new SelectListItem
            {
                Text = u.FormatName,
                Value = u.FormatName // Using name as the value based on your previous model
            });

           
            List<SelectListItem> statusList = new()
            {
                new SelectListItem { Text = "Upcoming", Value = "Upcoming" },
                new SelectListItem { Text = "Active", Value = "Active" },
                new SelectListItem { Text = "Completed", Value = "Completed" }
            };

            ViewBag.StatusList = statusList;
          
            if (id == null || id == 0)
            {
                //create
                Tournament obj = new Tournament();
                return View(obj);
            }
            else
            {
                //update
                Tournament? TournamentFromDb = _unitOfWork.Tournament.Get(u => u.TournamentId == id);
                return View(TournamentFromDb);
            }
        }

        [HttpPost]        
        public IActionResult Upsert(Tournament obj)
        {
            if(obj.TournamentId == 0)
            {
                // Create
                _unitOfWork.Tournament.Add(obj);
                TempData["success"] = "Tournament created successfully";
            }
            else
            {
                // Update
                _unitOfWork.Tournament.Update(obj);
                TempData["success"] = "Tournament updated successfully";
            }
            _unitOfWork.Save();
            //return RedirectToAction("Index");
            return RedirectToAction("Index");
        }

       
        // ======================
        // EDIT - GET
        // ======================
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Tournament? tournamentFromDb = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

            if (tournamentFromDb == null)
            {
                return NotFound();
            }

            return View(tournamentFromDb);          


        }

        // ======================
        // EDIT - POST
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Tournament obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Tournament.Update(obj);              
                _unitOfWork.Save();
                TempData["success"] = "Tournament updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }

        // ======================
        // DELETE - GET
        // ======================
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var tournamentFromDb = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

            if (tournamentFromDb == null)
            {
                return NotFound();
            }

            return View(tournamentFromDb);
        }

      
        [HttpPost, ActionName("Delete")]
       // [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Tournament.Remove(obj);
            _unitOfWork.Save();

            TempData["success"] = "Tournament deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        // ======================
        // DETAILS
        // ======================
        public IActionResult Details(int? id)
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

        public IActionResult AddTeam(int tournamentId)
        {
            var teams = _unitOfWork.Team.GetAll();

            ViewBag.Teams = teams;
            ViewBag.TournamentId = tournamentId;

            return View();
        }

        //public IActionResult Brackets(int id)
        //{
        //    var tournament = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

        //    // Use the NEW GetAll we created to include HomeTeam and AwayTeam data
        //    var allMatches = _unitOfWork.Match.GetAll(
        //        u => u.TournamentId == id,
        //        includeProperties: "HomeTeam,AwayTeam"
        //    );

        //    var viewModel = new TournamentBracketsVM
        //    {
        //        TournamentId = id,
        //        TournamentName = tournament.Name,

        //        // Filter by your 'Stage' property
        //        SemiFinals = allMatches.Where(m => m.Stage == "Semi-Final").Select(m => new BracketMatchDetail
        //        {
        //            //MatchId = m.MatchId,
        //            HomeTeamName = m.HomeTeam?.TeamName ?? "TBD",
        //            AwayTeamName = m.AwayTeam?.TeamName ?? "TBD",
        //            HomeScore = m.HomeScore,
        //            AwayScore = m.AwayScore
        //        }).ToList(),

        //        Final = allMatches.Where(m => m.Stage == "Final").Select(m => new BracketMatchDetail
        //        {
        //           // MatchId = m.MatchId,
        //            HomeTeamName = m.HomeTeam?.TeamName ?? "TBD",
        //            AwayTeamName = m.AwayTeam?.TeamName ?? "TBD",
        //            HomeScore = m.HomeScore,
        //            AwayScore = m.AwayScore
        //        }).FirstOrDefault()
        //    };

        //    return View(viewModel);
        //}

        //// Action for Live Scores
        //public IActionResult LiveScores(int id)
        //{
        //    var tournament = _unitOfWork.Tournament.Get(u => u.TournamentId == id);

        //    // Include the Team models so you can display names instead of IDs
        //    var liveMatches = _unitOfWork.Match.GetAll(
        //        u => u.TournamentId == id && (u.Status == "Live" || u.Status == "Upcoming"),
        //        includeProperties: "HomeTeam,AwayTeam"
        //    );

        //    return View(liveMatches);
        //}

        //// GET: Admin/Tournaments/UpdateScore/5
        //public IActionResult UpdateScore(int id)
        //{
        //    // Fetch the match and include team names so the admin knows who they are scoring
        //    var match = _unitOfWork.Match.Get(u => u.MatchId == id, includeProperties: "HomeTeam,AwayTeam");

        //    if (match == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(match);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateScore(Match obj)
        //{
            
        //    var matchFromDb = _unitOfWork.Match.Get(u => u.MatchId == obj.MatchId);

        //    if (matchFromDb == null)
        //    {
        //        return NotFound();
        //    }

        //    // 2. Only update the specific fields we allowed the Admin to change
        //    matchFromDb.HomeScore = obj.HomeScore;
        //    matchFromDb.AwayScore = obj.AwayScore;
        //    matchFromDb.Status = obj.Status;

        //    // 3. Save changes
        //    _unitOfWork.Save();

        //    TempData["success"] = "Match updated successfully!";

        //    // Redirect back to LiveScores for that specific tournament
        //    return RedirectToAction("LiveScores", new { id = matchFromDb.TournamentId });
        //}

        //public IActionResult MatchDetails(int id)
        //{
        //    // Fetch match with all related team info
        //    var match = _unitOfWork.Match.Get(u => u.MatchId == id,
        //        includeProperties: "HomeTeam,AwayTeam,Tournament");

        //    if (match == null) return NotFound();

        //    return View(match);
        //}

        //// GET: Admin/Tournaments/AddGoal?matchId=5
        //public IActionResult AddGoal(int matchId)
        //{
        //    var match = _unitOfWork.Match.Get(u => u.MatchId == matchId, includeProperties: "HomeTeam,AwayTeam");

        //    // We pass the match to the view so we can pick which team scored
        //    ViewBag.HomeTeamName = match.HomeTeam.TeamName;
        //    ViewBag.AwayTeamName = match.AwayTeam.TeamName;

        //    return View(new Goal { MatchId = matchId });
        //}

        public IActionResult ViewTournaments()
        {
            return View();
        }

        // GET: Show all pending registrations for a tournament
        public IActionResult ManageRegistrations(int tournamentId)
        {
            var registrations = _unitOfWork.TournamentRegistration
                .GetAll(r => r.TournamentId == tournamentId,
                        includeProperties: "Team,Tournament");

            return View(registrations);
        }

        // POST: Approve or Reject
        [HttpPost]
        public IActionResult ReviewRegistration(int registrationId, string decision, string? adminNotes)
        {
            var reg = _unitOfWork.TournamentRegistration
                .Get(r => r.RegistrationId == registrationId);

            if (reg == null) return NotFound();

            reg.Status = decision; // "Approved" or "Rejected"
            reg.ReviewedDate = DateTime.Now;
            reg.AdminNotes = adminNotes;

            _unitOfWork.TournamentRegistration.Update(reg);

            // If approved, also add to TournamentTeam (the actual participants table)
            if (decision == "Approved")
            {
                var alreadyJoined = _unitOfWork.TournamentTeams
                    .Get(t => t.TournamentId == reg.TournamentId && t.TeamId == reg.TeamId);

                if (alreadyJoined == null)
                {
                    _unitOfWork.TournamentTeams.Add(new TournamentTeam
                    {
                        TournamentId = reg.TournamentId,
                        TeamId = reg.TeamId,
                        DateJoined = DateTime.Now
                    });
                }
            }

            _unitOfWork.Save();
            TempData["success"] = $"Team {decision} successfully.";

            return RedirectToAction("ManageRegistrations", new { tournamentId = reg.TournamentId });
        }

        // Add this action to your existing TournamentsController.
        // Required using (add at top of TournamentsController.cs if not already there):
        //   using SoccerTournament.Models.Models.ViewModels;

        /// <summary>
        /// GET  /Admin/Tournaments/ViewMatches/5
        /// Shows ALL matches in a tournament for every team — admin overview.
        /// Linked from the "View Matches" button on the Tournament Details page.
        /// </summary>
    //    public IActionResult ViewMatches(int id)
    //    {
    //        var tournament = _unitOfWork.Tournament.Get(t => t.TournamentId == id);
    //        if (tournament == null) return NotFound();

    //        // All matches in this tournament, with team and goal data
    //        var matches = _unitOfWork.Match.GetAll(
    //            m => m.TournamentId == id,
    //            includeProperties: "HomeTeam,AwayTeam,Goals"
    //        ).OrderBy(m => m.MatchDate).ToList();

    //        // Stage display order
    //        var stageOrder = new List<string>
    //{
    //    "Group Stage", "Round of 16", "Quarter-Final", "Semi-Final", "Final"
    //};

    //        // Build detail objects — all CSS decisions made here
    //        var details = matches.Select(m =>
    //        {
    //            string statusBadge = m.Status == "Live" ? "abadge-live"
    //                               : m.Status == "Completed" ? "abadge-completed"
    //                               : "abadge-upcoming";

    //            string cardAccent = m.Status == "Live" ? "accent-live"
    //                               : m.Status == "Completed" ? "accent-done"
    //                               : "accent-upcoming";

    //            var goals = m.Goals
    //                .OrderBy(g => g.Minute)
    //                .Select(g => new AdminGoalDetail
    //                {
    //                    ScorerName = g.ScorerName,
    //                    Minute = g.Minute,
    //                    TeamId = g.TeamId,
    //                    IsPenalty = g.IsPenalty,
    //                    // Resolve team name here so view never compares IDs
    //                    TeamName = g.TeamId == m.HomeTeamId
    //                                 ? (m.HomeTeam?.TeamName ?? "")
    //                                 : (m.AwayTeam?.TeamName ?? "")
    //                }).ToList();

    //            return new AdminMatchDetail
    //            {
    //                MatchId = m.MatchId,
    //                MatchDate = m.MatchDate,
    //                Stage = m.Stage,
    //                Status = m.Status,
    //                HomeTeamId = m.HomeTeamId,
    //                HomeTeamName = m.HomeTeam?.TeamName ?? "TBD",
    //                HomeTeamLogo = m.HomeTeam?.LogoUrl,
    //                AwayTeamId = m.AwayTeamId,
    //                AwayTeamName = m.AwayTeam?.TeamName ?? "TBD",
    //                AwayTeamLogo = m.AwayTeam?.LogoUrl,
    //                HomeScore = m.HomeScore,
    //                AwayScore = m.AwayScore,
    //                StatusBadgeClass = statusBadge,
    //                CardAccentClass = cardAccent,
    //                Goals = goals
    //            };
    //        }).ToList();

    //        // Group by stage in defined order
    //        var grouped = details
    //            .GroupBy(m => m.Stage)
    //            .OrderBy(g => stageOrder.Contains(g.Key) ? stageOrder.IndexOf(g.Key) : 99)
    //            .Select(g => new AdminMatchStageGroup
    //            {
    //                Stage = g.Key,
    //                Matches = g.ToList()
    //            })
    //            .ToList();

    //        var vm = new TournamentMatchesVM
    //        {
    //            TournamentId = id,
    //            TournamentName = tournament.Name,
    //            TournamentStatus = tournament.Status,
    //            GroupedMatches = grouped
    //        };

    //        return View(vm);
    //    }

    }
}
      
