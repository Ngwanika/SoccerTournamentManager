using Microsoft.AspNetCore.Mvc;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using Microsoft.AspNetCore.Identity;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {

        //private readonly UserManager<ApplicationUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;

        //public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        //{
        //    _userManager = userManager;
        //    _roleManager = roleManager;
        //}


        public IActionResult Index()
        {
          //  var userList = _userManager.Users.ToList();
            return View();
        }

        #region API CALLS
        public IActionResult GetAll()
        {
            //   var userList = _userManager.Users.ToList();
            //  return Json(new { data = userList });
            return View();
        }
        #endregion
    }
}
