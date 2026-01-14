using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Schedia.Web.Blazor.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/set-cookie", async (HttpContext context, SetCookieRequest request) =>
        {
            try
            {
                // Parse JWT token to extract claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(request.AccessToken);

                var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Sign in with cookie
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = request.ExpiresIn
                    });

                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
        }).AllowAnonymous();

        app.MapPost("/api/auth/clear-cookie", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { success = true });
        }).AllowAnonymous();
    }

    public record SetCookieRequest(string AccessToken, DateTimeOffset ExpiresIn);
}