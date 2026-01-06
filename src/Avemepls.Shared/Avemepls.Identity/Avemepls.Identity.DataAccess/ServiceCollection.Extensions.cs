using System;

using Avemepls.Core.DataAccess.ContextInitializing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Identity.DataAccess;

// #PoIgnore#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder>? configure)
    {
        serviceCollection.AddDataAccess((_, cfg) => configure?.Invoke(cfg));

        return serviceCollection;
    }

    public static IServiceCollection AddDataAccess(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IDbContextFactory<IdentityDbContext>>().CreateDbContext());
        services.AddDbContextFactory<IdentityDbContext>(configure, ServiceLifetime.Scoped);
        services.AddScoped<IContextInitializer, ContextInitializer>();

        return services;
    }
}