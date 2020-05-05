using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.ScrimTracker.Api.Controllers
{
    public class BungieController : Controller
    {
        private readonly IBungieApiAdapter _adapter;
        
        public BungieController(IBungieApiAdapter adapter)
        {
            _adapter = adapter;
        }
        public async Task<string> History()
        {
            return await _adapter.GetCharacterHistory();
        }
    }
}