using Avemepls.Blazor;
using Avemepls.Blazor.Common.Menus;
using Avemepls.Domain;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Security;
using Avemepls.Security.Permissions;
using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using Schedia.Auth.Domain;
using Schedia.Domain.Core;
using Schedia.Identity.Domain;
using Schedia.Web.Blazor.Services;
using Schedia.Web.Core.Blazor;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;
using Schedia.Web.Shared.FluentValidationCustoms;

namespace Schedia.Web.Blazor;

// #PoIgnore#
public static class ProgramExtensions
{
    public static IServiceCollection AddSchediaBase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatR()
            .AddModules(configuration)
            .AddDomainCore()
            .OverrideValidationMessages()

            // .AddDatabasePipelines()
            // .OverrideValidationMessages()
            // .AddMinioStorage(configuration)
            ;

        services.AddDataAccess((_, cfg) =>
        {
            cfg
                .UseNpgsql(configuration.GetConnectionString("Schedia"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services
            .AddMemoryCache()
            .AddSingleton<ICurrentDateTimeProvider, CurrentSystemDateTimeProvider>()
            .AddScoped<PermissionRegistry>()
            .AddHttpContextAccessor();

        services.AddPermissions(cfg => cfg.AddAssemblies(typeof(Program).Assembly));

        services.AddMenuSections(builder =>
        {
            // builder.WithSection("audit-log",
            //         "Аудит-лог",
            //         new MenuItemModel("audit-log", "audit-log", "События", AuditorPermissions.Auditor.View))
            //     .WithIcon("read");
            builder.WithSection("tools",
                    "Инструменты",
                    new MenuItemModel("jobs",
                        "jobs-view",
                        "Фоновые задания"))
                .WithIcon("tool");

            builder.WithSection("identity",
                    "Пользователи",
                    new MenuItemModel("users",
                        "/administration/users",
                        "Пользователи"))
                .WithIcon("tool");
        });

        return services;
    }

    private static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSchediaAuth(configuration)
            .AddSchediaIdentity();
    }

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
        builder.Services.AddBlazorPages(typeof(Schedia.Web.Shared.Services.IPlatformService).Assembly);
        builder.Services.AddAntDesign();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<Schedia.Web.Shared.Services.IPlatformService, WebPlatformService>();
        builder.Services.AddScoped<Schedia.Web.Shared.Services.IAuthenticationService, WebAuthenticationService>();
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IPrincipalAccessor, BlazorPrincipalAccessor>();

        builder.Services
            .AddRazorComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment())
            .AddInteractiveServerComponents();

        builder.WebHost.UseStaticWebAssets(); // for include blazor _content
    }
}