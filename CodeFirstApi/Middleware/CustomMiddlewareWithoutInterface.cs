using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CodeFirstApi.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CustomMiddlewareWithoutInterface(RequestDelegate next, ILogger<CustomMiddlewareWithoutInterface> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger _logger = logger;
        //private readonly ILoggerFactory _loggerFactory;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            //var log = _logger.CreateLogger<CustomMiddlewareWithoutInterface>();
            _logger.LogInformation($"CustomMiddlewareWithoutInterface: {httpContext.Response.StatusCode}");
            await _next(httpContext);
        }
    }
}
