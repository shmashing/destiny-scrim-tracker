﻿using System.Linq;
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
        public IActionResult Get()
        {
            var guardianSnapshots = _guardianService.GetGuardians();
            return View(guardianSnapshots);
        }

        [HttpGet("/add_guardian")]
        [Authorize]
        public IActionResult AddGuardianView()
        {
            return View();
        }
        
        [HttpPost("/add_guardian")]
        [Authorize]
        public IActionResult AddGuardian([FromForm] CreateGuardianRequest request)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddGuardianView");
            }
            
            var guardian = request.ToGuardian();
            _guardianService.CreateGuardian(guardian);
            return Redirect("/guardian");
        }
        
        [HttpGet("{guardianId}")]
        public IActionResult GetGuardian(string guardianId)
        {
            var guardian = _guardianService.GetGuardian(guardianId);
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
        public IActionResult Put(UpdateGuardianRequest request, [FromRoute] string guardianId)
        {
            var guardian = request.ToGuardian(guardianId);
            var updatedGuardian = _guardianService.UpdateGuardian(guardian);
            
            return Redirect($"guardians/{updatedGuardian.Id}");
        }

        [HttpDelete("{guardianId}")]
        [Authorize]
        public IActionResult Delete([FromRoute] string guardianId)
        {
            var guardian = _guardianService.DeleteGuardian(guardianId);
            return Json(guardian);
        }
    }
}