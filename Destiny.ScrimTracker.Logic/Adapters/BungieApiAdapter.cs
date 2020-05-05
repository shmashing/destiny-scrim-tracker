using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Models;
using Newtonsoft.Json;

namespace Destiny.ScrimTracker.Logic.Adapters
{
    public interface IBungieApiAdapter
    {
        Task<string> GetCharacterHistory();
    }
    
    public class BungieApiAdapter : IBungieApiAdapter
    {
        private const string _bungieClientName = "bungie";
        private const string _accountId = "4611686018453600035";
        private const string _characterId = "2305843009261022530";
        private const string _gameMode = "32";
        
        private readonly IHttpClientFactory _httpClientFactory;
        
        public BungieApiAdapter(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        public async Task<string> GetCharacterHistory()
        {
            var client = _httpClientFactory.CreateClient(_bungieClientName);

            var uri = string.Format(BungieApiEndpoints.ActivityHistoryByCharacter, _accountId, _characterId, _gameMode);
            var response = await client.GetAsync(uri);

            return JsonConvert.SerializeObject(response);
        }
    }
}