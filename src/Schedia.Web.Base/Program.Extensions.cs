using Avemepls.Blazor;
using Avemepls.Blazor.Common.Menus;
using Avemepls.Domain;
using Avemepls.Infrastructure.DateTime;
using Avemepls.RsLocalizer.Extensions;
using Avemepls.Security;
using Avemepls.Security.Permissions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Schedia.Auth.Application;
using Schedia.DataAccess;
using Schedia.Domain.Core;
using Schedia.Identity.Domain;
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
            .AddSchediaAuthApplication(configuration)
            .AddSchediaIdentityDomain();
    }
}