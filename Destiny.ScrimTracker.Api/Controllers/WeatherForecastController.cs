using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuardianController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<GuardianController> _logger;

        public GuardianController(ILogger<GuardianController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{guardianName}")]
        public string Get(string guardianName)
        {
            return $"fetching guardian: {guardianName}";
        }
    }
}
