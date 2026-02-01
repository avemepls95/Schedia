using Avemepls.Core.DataAccess.ContextInitializing;
using Avemepls.Identity.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Schedia.DataAccess;

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
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IDbContextFactory<SchediaDbContext>>().CreateDbContext());
        services.AddDbContextFactory<SchediaDbContext>(configure, ServiceLifetime.Scoped);
        services.AddScoped<IContextInitializer, ContextInitializer>();

        services.AddIdentityDataAccess(configure);

        return services;
    }
}