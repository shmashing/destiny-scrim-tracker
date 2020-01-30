using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Destiny.ScrimTracker.Api.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [Route("")]
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
    }
}