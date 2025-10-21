using System; // For StringComparison
using System.IdentityModel.Tokens.Jwt; // For JwtRegisteredClaimNames
using System.Security.Claims; // For ClaimTypes
using System.Threading.Tasks; // For Task
using Microsoft.AspNetCore.Http; // For HttpContext, RequestDelegate, StatusCodes
using Microsoft.Extensions.Caching.Memory; // For IMemoryCache
using Modules.Users.Domain.Tokens; // For RevocatedTokenType

namespace Modules.Users.Features.Middlewares;

/// <summary>
/// Checks if the JWT presented in the request has been revoked (e.g., due to role change).
/// </summary>
public class CheckRevocatedTokensMiddleware(RequestDelegate next, IMemoryCache memoryCache)
{
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _memoryCache = memoryCache; // Used to quickly check for revoked JTIs

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow anonymous access to login and refresh endpoints
        if (context.Request.Path.StartsWithSegments("/api/users/login", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.StartsWithSegments("/api/users/refresh", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Get the unique identifier (JTI) of the current JWT
        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        // If no JTI (e.g., anonymous user or token missing claim), proceed
        if (string.IsNullOrEmpty(jti))
        {
            await _next(context);
            return;
        }

        // Check the memory cache if this JTI has been revoked
        var revocationType = _memoryCache.Get<RevocatedTokenType?>(jti);
        if (revocationType.HasValue)
        {
            // Token is revoked, block the request
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token has been revoked."); // Optional message
            return;
        }

        // Token is valid, continue the pipeline
        await _next(context);
    }
}