using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Enums;

namespace MyProject.Middleware;

public class UserStatusMiddleware : IMiddleware
{
    private readonly IUserRepository _users;

    public UserStatusMiddleware(IUserRepository users)
    {
        _users = users;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Auth endpoints ochiq
        if (path.StartsWith("/api/auth/register") ||
            path.StartsWith("/api/auth/login") ||
            path.StartsWith("/api/auth/confirm"))
        {
            await next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userIdStr = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        var user = await _users.GetByIdAsync(userId, context.RequestAborted);

        // Deleted bo'lsa
        if (user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User not found (deleted).");
            return;
        }

        // Blocked bo'lsa
        if (user.Status == UserStatus.Blocked)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User is blocked.");
            return;
        }

        // MUHIM: Unverified user'ni bloklamaymiz (task bo‘yicha login/manage mumkin)
        await next(context);
    }
}
