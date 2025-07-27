using FamilyTaskManagement.Web.Services;
using System.Security.Claims;

namespace FamilyTaskManagement.Web.Middleware
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenMiddleware> _logger;

        public TokenMiddleware(RequestDelegate next,ILogger<TokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context,IApiService apiService)
        {
            try
            {
                // Check if user is authenticated and has access token
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var accessToken = context.User.FindFirst("access_token")?.Value;

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        // Set the token in ApiService for this request
                        //apiService.SetAuthorizationToken(accessToken);
                        _logger.LogDebug("Token set for authenticated user: {UserId}",
                            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Authenticated user has no access token in claims");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error in TokenMiddleware");
            }

            await _next(context);
        }
    }

    public static class TokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenMiddleware>( );
        }
    }
}
