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
            _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp.ToString());

            var ip = _adminSafeList.Split(';');
            
            var badIp = !ip.Contains(remoteIp.ToString());

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