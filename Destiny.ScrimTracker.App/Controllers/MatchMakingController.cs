using System;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Destiny.ScrimTracker.App.Controllers
{
    public class MatchMakingController : Controller
    {
        private readonly IGuardianService _guardianService;
        private readonly IMatchMakingService _matchMakingService;

        public MatchMakingController(IGuardianService guardianService, IMatchMakingService matchMakingService)
        {
            _guardianService = guardianService;
            _matchMakingService = matchMakingService;
        }
        
        // GET
        public IActionResult Index()
        {
            var guardians = _guardianService.GetGuardians().OrderBy(g => g.Guardian.GamerTag);

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            
            return View(guardians);
        }

        [HttpPost]
        public IActionResult MatchMake()
        {
            var guardianKeys = Request.Form.Keys.Where(k => k.Contains("guardian"));
            var teamSizeIsValidInput = int.TryParse(Request.Form["teamSize"], out var teamSize);
            var guardianIds = guardianKeys.Select(key => Request.Form[key]).Select(dummy => (string) dummy).ToList();

            if (!teamSizeIsValidInput)
            {
                TempData["Error"] = "Team size can not be left blank.";
                return RedirectToAction("Index");
            }

            if (guardianIds.Count() % teamSize != 0)
            {
                TempData["Error"] = "Number of guardians selected and team sizes don't match up.";
                return RedirectToAction("Index");
            }
            
            var teams = _matchMakingService.MatchTeams(guardianIds, teamSize);
            
            return View(teams);
        }
    }
}