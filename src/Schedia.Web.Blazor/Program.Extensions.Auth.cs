using System.Security.Claims;

using Avemepls.Auth.Abstractions;
using Avemepls.Blazor;
using Avemepls.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Schedia.Web.Blazor.Endpoints;
using Schedia.Web.Blazor.Services;
using Schedia.Web.Core.Blazor;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

namespace Schedia.Web.Blazor;

public static class AuthProgramExtensions
{
    public static void AddAuth(this WebApplicationBuilder builder)
    {
        // Register Cookie + Google Authentication
        var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "Schedia.Auth";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        var googleOptions = builder.Configuration.GetSection("Auth:Google").Get<GoogleAuthOptions>();
        if (googleOptions is { ClientId.Length: > 0 })
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.CallbackPath = "/api/auth/google-callback";
                options.ClaimActions.MapJsonKey(GoogleClaims.EmailVerified, "email_verified", ClaimValueTypes.Boolean);
            });
        }

        // Register custom authentication state provider for Bearer tokens
        builder.Services.AddScoped<TokenAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<TokenAuthenticationStateProvider>());

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<Schedia.Web.Base.Services.IAuthenticationService, WebAuthenticationService>();

        builder.Services.AddScoped<IPrincipalAccessor, BlazorPrincipalAccessor>();
    }
}