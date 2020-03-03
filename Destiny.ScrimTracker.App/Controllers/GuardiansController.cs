using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.App.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Destiny.ScrimTracker.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuardiansController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGuardianService _guardianService;
        private readonly ILogger<GuardiansController> _logger;

        public GuardiansController(IMatchService matchService, IGuardianService guardianService, ILogger<GuardiansController> logger)
        {
            _matchService = matchService;
            _guardianService = guardianService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var guardianSnapshots = await _guardianService.GetGuardians();
            return View(guardianSnapshots);
        }

        [HttpGet("/new")]
        [Authorize]
        public IActionResult AddGuardianView()
        {
            return View();
        }
        
        [HttpPost("/new")]
        [Authorize]
        public IActionResult AddGuardian([FromForm] CreateGuardianRequest request)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddGuardianView");
            }
            
            var guardian = request.ToGuardian();
            _guardianService.CreateGuardian(guardian);
            return Redirect("/guardians");
        }
        
        [HttpGet("{guardianId}")]
        public async Task<IActionResult> GetGuardian(string guardianId)
        {
            var guardian = await _guardianService.GetGuardian(guardianId);
            var guardianElo = _guardianService.GetGuardianElo(guardianId);
            var guardianEfficiency = _guardianService.GetGuardianEfficiency(guardianId);
            var matchCount = _matchService.GetMatchResultsForGuardian(guardianId).Count();
            
            var guardianHistory = new GuardianHistory()
            {
                Guardian = guardian,
                EfficiencyHistory = guardianEfficiency,
                EloHistory = guardianElo,
                MatchCount = matchCount
            };
            
            return View(guardianHistory);
        }

        [HttpPut("{guardianId}")]
        [Authorize]
        public async Task<IActionResult> Put(UpdateGuardianRequest request, [FromRoute] string guardianId)
        {
            var guardian = request.ToGuardian(guardianId);
            var updatedGuardian = await _guardianService.UpdateGuardian(guardian);
            
            return Redirect($"guardians/{updatedGuardian.Id}");
        }

        [HttpDelete("{guardianId}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] string guardianId)
        {
            var guardian = await _guardianService.DeleteGuardian(guardianId);
            return Json(guardian);
        }
    }
}
