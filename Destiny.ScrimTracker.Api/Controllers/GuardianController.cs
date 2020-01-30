﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Api.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuardianController : Controller
    {
        private readonly IGuardianService _guardianService;
        private readonly ILogger<GuardianController> _logger;

        public GuardianController(IGuardianService guardianService, ILogger<GuardianController> logger)
        {
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
        public IActionResult AddGuardianView()
        {
            return View();
        }
        
        [HttpPost("/add_guardian")]
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
        public GuardianResponse GetGuardian(string guardianId)
        {
            var guardian = _guardianService.GetGuardian(guardianId);
            var guardianElo = _guardianService.GetGuardianElo(guardianId);
            return new GuardianResponse()
            {
                Guardian = guardian,
                GuardianElo = guardianElo
            };
        }

        [HttpPut("{guardianId}")]
        public Guardian Put(UpdateGuardianRequest request, [FromRoute] string guardianId)
        {
            var guardian = request.ToGuardian(guardianId);
            return _guardianService.UpdateGuardian(guardian);
        }

        [HttpDelete("{guardianId}")]
        public string Delete([FromRoute] string guardianId)
        {
            return _guardianService.DeleteGuardian(guardianId);
        }
    }
}
