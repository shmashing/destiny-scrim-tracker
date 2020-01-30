using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("")]
    public class HomeController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGuardianService _guardianService;

        public HomeController(IMatchService matchService, IGuardianService guardianService)
        {
            _matchService = matchService;
            _guardianService = guardianService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Microsoft.AspNetCore.Mvc.Route("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}