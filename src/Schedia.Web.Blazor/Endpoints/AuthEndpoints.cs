using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Avemepls.Auth.Domain.ViaGoogle;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace Schedia.Web.Blazor.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/set-cookie", async (HttpContext context, SetCookieRequest request) =>
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(request.AccessToken);

                var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

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

        app.MapGet("/api/auth/google-login", (string? returnUrl) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/google-response?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}"
            };
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        }).AllowAnonymous();

        app.MapGet("/api/auth/google-response", async (HttpContext context, IMediator mediator, string? returnUrl) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Results.Redirect("/login");
            }

            var claims = result.Principal!.Claims.ToList();
            var googleId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var email = claims.First(c => c.Type == ClaimTypes.Email).Value;
            var name = claims.Find(c => c.Type == ClaimTypes.Name)?.Value;

            var token = await mediator.Send(new LoginViaGoogle.Command
            {
                GoogleId = googleId,
                Email = email,
                DisplayName = name
            });

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token.AccessToken);
            var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = token.ExpiresIn
                });

            return Results.Redirect(returnUrl ?? "/");
        }).AllowAnonymous();
    }

    public record SetCookieRequest(string AccessToken, DateTimeOffset ExpiresIn);
}