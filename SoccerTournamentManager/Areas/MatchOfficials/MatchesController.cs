using Microsoft.AspNetCore.Mvc;

namespace SoccerTournamentManager.Areas.MatchOfficials
{
    public class MatchesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
