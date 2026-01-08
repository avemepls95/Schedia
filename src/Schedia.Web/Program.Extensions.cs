using Avemepls.Blazor;
using Avemepls.Blazor.Common.Menus;
using Avemepls.Domain;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Security;
using Avemepls.Security.Permissions;
using Avemepls.Security.Principal;

using Microsoft.EntityFrameworkCore;

using Schedia.Auth.Domain;
using Schedia.Web.Core.Blazor;

namespace Schedia.Web;

// #PoIgnore#
public static class ProgramExtensions
{
    public static IServiceCollection AddSchediaBase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatR()
            .AddSchediaAuth(configuration)

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
            .AddScoped<IPrincipalAccessor, BlazorPrincipalAccessor>();

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

            builder.WithMenuItem("logout", "Выйти", "/api/auth/oidc/sign-out", null)
                .WithIcon("logout");
        });

        return services;
    }

    public static void AddSchediaAdmin(this WebApplicationBuilder builder)
    {
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddBlazorCore();
        builder.Services.AddAntDesign();
        builder.Services.AddRazorPages();
        builder.Services.AddRazorComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment()).AddInteractiveServerComponents();

        builder.WebHost.UseStaticWebAssets(); // for include blazor _content
    }
}