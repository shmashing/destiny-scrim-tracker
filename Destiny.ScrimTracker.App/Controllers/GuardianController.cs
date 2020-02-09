﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.App.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Destiny.ScrimTracker.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuardianController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGuardianService _guardianService;
        private readonly ILogger<GuardianController> _logger;

        public GuardianController(IMatchService matchService, IGuardianService guardianService, ILogger<GuardianController> logger)
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
        public Guardian Put(UpdateGuardianRequest request, [FromRoute] string guardianId)
        {
            var guardian = request.ToGuardian(guardianId);
            return _guardianService.UpdateGuardian(guardian);
        }

        [HttpPost("{guardianId}")]
        [Authorize]
        public IActionResult Delete([FromRoute] string guardianId)
        {
            var guardian = _guardianService.DeleteGuardian(guardianId);
            return RedirectToAction("Get");
        }
    }
}