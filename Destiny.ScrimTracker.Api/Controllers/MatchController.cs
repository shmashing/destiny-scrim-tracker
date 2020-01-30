using Destiny.ScrimTracker.Api.Requests;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }
        

        [HttpPost]
        public string Post(CreateMatchRequest request)
        {
            var match = request.ToMatch();
            return _matchService.CreateMatch(match, request.Teams);
        }

        [HttpDelete]
        [Route("{matchId}")]
        public string Delete([FromRoute] string matchId)
        {
            return _matchService.DeleteMatch(matchId);
        }
    }
}