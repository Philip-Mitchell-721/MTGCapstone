using Microsoft.CodeAnalysis.VisualBasic.Syntax;

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
            //ASK: Do these two ifs check the same thing?  How does the second one work?
            //if (context.User.Identity is not null && context.User.Identity.IsAuthenticated)
            //{

            //}
            if (context.User.Identity is { IsAuthenticated: true})
            {
                var user = context.User;
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
