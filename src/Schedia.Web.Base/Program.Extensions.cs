using Avemepls.Auditor.DataAccess.DbContextAuditor;
using Avemepls.Auth.Application;
using Avemepls.Blazor;
using Avemepls.Blazor.Common.Menus;
using Avemepls.Domain;
using Avemepls.Identity.Application;
using Avemepls.Infrastructure.DateTime;
using Avemepls.RsLocalizer.Extensions;
using Avemepls.Security;
using Avemepls.Security.Permissions;
using Avemepls.Security.Principal;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Schedia.Core;
using Schedia.DataAccess;
using Schedia.Domain.Core;
using Schedia.Web.Base.FluentValidationCustoms;

namespace Schedia.Web.Base;

// #RsIgnore#
public static class ProgramExtensions
{
    public static IServiceCollection AddSchediaBase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatR()
            .AddModules(configuration)
            .AddDomainCore()
            .AddSchediaCore(configuration)
            .OverrideValidationMessages()
            .AddPortableObjectLocalization(cfg =>
            {
                cfg.ResourcesPath = "Locale";
                cfg.ResourcesDirectory = AppContext.BaseDirectory;
            })

            // .AddDatabasePipelines()
            // .OverrideValidationMessages()
            // .AddMinioStorage(configuration)
            ;

        services.AddDataAccess((serviceProvider, cfg) =>
        {
            cfg
                .UseNpgsql(configuration.GetConnectionString("Schedia"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            cfg.AddInterceptors(new DbContextAuditorInterceptor(
                serviceProvider.GetRequiredService<IMediator>(),
                [.. serviceProvider.GetRequiredService<IOptions<AuditorConfig>>().Value.AuditableTypes],
                serviceProvider.GetRequiredService<ICurrentDateTimeProvider>(),
                serviceProvider.GetRequiredService<IPrincipalAccessor>()));
        });

        services
            .AddMemoryCache()
            .AddSingleton<ICurrentDateTimeProvider, CurrentSystemDateTimeProvider>()
            .AddScoped<PermissionRegistry>()
            .AddHttpContextAccessor();

        services.AddPermissions(cfg => cfg.AddAssemblies(typeof(ProgramExtensions).Assembly));

        services.AddSchediaMassTransit(configuration);

        services.AddMenuSections(builder =>
        {
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
            .AddAuthApplication(configuration)
            .AddIdentityApplication(configuration);
    }
}