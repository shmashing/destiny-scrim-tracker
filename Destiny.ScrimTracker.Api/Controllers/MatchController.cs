using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Destiny.ScrimTracker.Api.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGuardianService _guardianService;

        public MatchController(IMatchService matchService, IGuardianService guardianService)
        {
            _matchService = matchService;
            _guardianService = guardianService;
        }
        
        // GET
        public IActionResult Index()
        {
            var matchResults = _matchService.GetMatchResults();
            return View(matchResults);
        }

        [Authorize]
        [Route("match")]
        public IActionResult AddMatchView()
        {
            return View();
        }
        
        [Authorize]
        [Route("add_match")]
        public IActionResult CreateMatchForm([FromQuery] int numOfTeams, [FromQuery] int playersPerTeam)
        {
            var guardians = _guardianService.GetGuardians();
            ViewData.Add("Guardians", guardians.OrderBy(g => g.Guardian.GamerTag).Select(g => g.Guardian.GamerTag));
            ViewData.Add("NumberOfTeams", numOfTeams);
            ViewData.Add("PlayersPerTeam", playersPerTeam);

            var match = new CreateMatchFormModel();

            var teams = new List<Team>();
            for (var i = 0; i < numOfTeams; i++)
            {
                teams.Add(new Team());
                teams[i].MatchResults = new List<MatchResult>();
                for (var j = 0; j < playersPerTeam; j++)
                {
                    teams[i].MatchResults.ToList().Add(new MatchResult());
                }
            }

            match.Teams = teams;
            return View(match);
        }

        [HttpPost]
        [Authorize]
        [Route("add_match")]
        public IActionResult FormatRequestAndAddMatch([FromQuery] int numOfTeams, [FromQuery] int playersPerTeam)
        {
            var createMatchRequest = new CreateMatchRequest();

            createMatchRequest.MatchType = Enum.Parse<MatchType>(Request.Form["MatchType"]);
            var teams = new List<MatchTeam>();
            for (var i = 0; i < numOfTeams; i++)
            {
                var team = new MatchTeam();
                team.Name = Request.Form[$"Team[{i}].Name"];
                team.TeamScore = int.Parse(Request.Form[$"Team[{i}].TeamScore"]);
                
                var guardianResults = new List<GuardianMatchResult>();
                for (var j = 0; j < playersPerTeam; j++)
                {
                    var results = new GuardianMatchResult
                    {
                        Kills = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Kills"]),
                        Assists = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Assists"]),
                        Deaths = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Deaths"]),
                        GuardianName = Request.Form[$"Team[{i}].MatchResults[{j}].GuardianName"]
                    };

                    if (results.Deaths == 0)
                    {
                        results.Efficiency = 0;
                    }
                    else
                    {
                        results.Efficiency = (double) (results.Kills + results.Assists) / results.Deaths;
                    }
                    Console.WriteLine($"Calculated Eff = {results.Efficiency:F4}");
                    guardianResults.Add(results);
                }

                team.GuardianMatchResults = guardianResults;
                teams.Add(team);
            }

            createMatchRequest.Teams = teams;

            _matchService.CreateMatch(createMatchRequest.ToMatch(), createMatchRequest.Teams);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public string Post(CreateMatchRequest request)
        {
            var match = request.ToMatch();
            return _matchService.CreateMatch(match, request.Teams);
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete([FromQuery] string matchId)
        {
            _matchService.DeleteMatch(matchId);
            return RedirectToAction("Index");
        }
    }
}