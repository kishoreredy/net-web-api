namespace CodeFirstApi.Middleware
{

    public class CustomInterfaceMiddleware(ILogger<CustomInterfaceMiddleware> log) : IMiddleware
    {
        private readonly ILogger<CustomInterfaceMiddleware> _log = log;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _log.LogInformation($"context: {context.Response} customMiddleware");
            await next(context);
        }
    }
}
