using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Destiny.ScrimTracker.Api.Middleware
{
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        private readonly string _adminSafeList;

        public AdminSafeListMiddleware(
            RequestDelegate next, 
            ILogger<AdminSafeListMiddleware> logger, 
            string adminSafeList)
        {
            _adminSafeList = adminSafeList;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

            var ip = _adminSafeList.Split(';');
            var bytes = remoteIp.GetAddressBytes();
            var badIp = true;
            
            _logger.LogDebug("Admin Safe List: {safeList}", _adminSafeList);            
            
            foreach (var address in ip)
            {
                var testIp = IPAddress.Parse(address);
                if(testIp.GetAddressBytes().SequenceEqual(bytes))
                {
                    badIp = false;
                    break;
                }
            }

            if(badIp) 
            {
                _logger.LogInformation("Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = 401;
                return;
            }

            await _next.Invoke(context);
        }
    }
}