using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Avemepls.Auth.Domain.ViaGoogle;
using Avemepls.Core.Extensions;

using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.WebUtilities;

namespace Schedia.Web.Blazor.Endpoints;

public static class AuthEndpoints
{
    private const string LoginPath = "/login";
    private const string GoogleResponsePath = "/api/auth/google-response";

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
            var safeReturnUrl = ReturnUrlValidator.GetSafeReturnUrl(returnUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = QueryHelpers.AddQueryString(GoogleResponsePath, "returnUrl", safeReturnUrl)
            };
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        }).AllowAnonymous();

        app.MapGet("/api/auth/google-response", async (HttpContext context, IMediator mediator, string? returnUrl) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Results.Redirect($"{LoginPath}?error=google_auth_failed");
            }

            var claims = result.Principal!.Claims.ToArray();
            var googleId = claims.Find(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.Find(c => c.Type == ClaimTypes.Email)?.Value;
            var emailVerified = claims.Find(c => c.Type == GoogleClaims.EmailVerified)?.Value;
            var firstName = claims.Find(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.Find(c => c.Type == ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);
                return Results.Redirect($"{LoginPath}?error=google_claims_missing");
            }

            if (!string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            {
                await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);
                return Results.Redirect($"{LoginPath}?error=google_email_unverified");
            }

            var token = await mediator.Send(new LoginViaGoogle.Command
            {
                GoogleId = googleId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
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

            return Results.Redirect(ReturnUrlValidator.GetSafeReturnUrl(returnUrl));
        }).AllowAnonymous();
    }

    public record SetCookieRequest(string AccessToken, DateTimeOffset ExpiresIn);
}