using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Security.Claims;

namespace MTGCapstone.API.Middleware
{
    public class LoggingUserScope
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingUserScope> _logger;

        public LoggingUserScope(RequestDelegate next, ILogger<LoggingUserScope> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            //if (context.User.Identity is { IsAuthenticated: true})
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                ClaimsPrincipal user = context.User;
                using (_logger.BeginScope("User:{user}", user.Identity.Name))
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
