using Avemepls.Core.DataAccess.ContextInitializing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Identity.DataAccess;

// #PoIgnore#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityDataAccess(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder>? configure)
    {
        serviceCollection.AddIdentityDataAccess((_, cfg) => configure?.Invoke(cfg));

        return serviceCollection;
    }

    public static IServiceCollection AddIdentityDataAccess(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IDbContextFactory<IdentityDbContext>>().CreateDbContext());
        services.AddDbContextFactory<IdentityDbContext>(configure, ServiceLifetime.Scoped);
        services.AddScoped<IContextInitializer, ContextInitializer>();

        return services;
    }
}