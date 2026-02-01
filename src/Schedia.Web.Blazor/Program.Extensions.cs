using Avemepls.Blazor;
using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor.Services;

using Schedia.Web.Blazor.Services;
using Schedia.Web.Core.Blazor;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

namespace Schedia.Web.Blazor;

public static class ProgramExtensions
{
    public static void AddBlazor(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        // Register HttpClient for API calls
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri)
        });

        // Register Cookie Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

        // Register custom authentication state provider for Bearer tokens
        builder.Services.AddScoped<TokenAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<TokenAuthenticationStateProvider>());

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddBlazorCore();
        builder.Services.AddBlazorPages(typeof(Schedia.Web.Base.Services.IPlatformService).Assembly);
        builder.Services.AddAntDesign();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<Schedia.Web.Base.Services.IPlatformService, WebPlatformService>();
        builder.Services.AddScoped<Schedia.Web.Base.Services.IAuthenticationService, WebAuthenticationService>();
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IPrincipalAccessor, BlazorPrincipalAccessor>();

        builder.Services
            .AddRazorComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment())
            .AddInteractiveServerComponents();

        builder.WebHost.UseStaticWebAssets(); // for include blazor _content
    }
}