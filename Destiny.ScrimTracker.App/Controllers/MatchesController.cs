using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.App.Requests;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGuardianService _guardianService;

        public MatchesController(IMatchService matchService, IGuardianService guardianService)
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
        [Route("new")]
        public async Task<IActionResult> CreateMatchForm()
        {
            var guardians = await _guardianService.GetGuardians();
            ViewData.Add("Guardians", guardians.OrderBy(g => g.Guardian.GamerTag).Select(g => g.Guardian.GamerTag));

            var match = new CreateMatchFormModel();
            return View(match);
        }

        [HttpPost]
        [Authorize]
        [Route("new")]
        public IActionResult FormatRequestAndAddMatch([FromQuery] int numOfTeams, [FromQuery] int playersPerTeam)
        {
            var createMatchRequest = new CreateMatchRequest();

            createMatchRequest.MatchType = Enum.Parse<MatchType>(Request.Form["MatchType"]);

            var formKeys = Request.Form.Keys;

            Console.WriteLine($"Key count: {formKeys.Count()}");
            var teams = new List<MatchTeam>();
            for (var i = 0; i < 2; i++)
            {
                var team = new MatchTeam();
                team.Name = Request.Form[$"Team[{i}].Name"];
                team.TeamScore = int.Parse(Request.Form[$"Team[{i}].TeamScore"]);

                var guardianCount = formKeys.Count(g => g.Contains($"Team[{i}]") && g.Contains("GuardianName"));
                Console.WriteLine($"Guardians on team: {guardianCount}");
                var guardianResults = new List<GuardianMatchResult>();
                for (var j = 0; j < guardianCount; j++)
                {
                    var results = new GuardianMatchResult
                    {
                        Kills = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Kills"]),
                        Assists = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Assists"]),
                        Deaths = int.Parse(Request.Form[$"Team[{i}].MatchResults[{j}].Deaths"]),
                        GuardianName = Request.Form[$"Team[{i}].MatchResults[{j}].GuardianName"]
                    };

                    Console.WriteLine($"Guardian results processed successfully: {results.GuardianName}");
                    if (results.Deaths == 0)
                    {
                        results.Efficiency = results.Kills + results.Assists;
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
        public async Task<string> Post(CreateMatchRequest request)
        {
            var match = request.ToMatch();
            return await _matchService.CreateMatch(match, request.Teams);
        }

        [HttpDelete("{matchId}")]
        [Authorize]
        public IActionResult Delete([FromRoute] string matchId)
        {
            var match = _matchService.DeleteMatch(matchId);
            return Json(match);
        }
    }
}