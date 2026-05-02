using Avemepls.ServiceBus.Common;
using Avemepls.ServiceBus.Requests;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.ServiceBus;

public static class Extensions
{
    /// <summary>
    /// Подключать если используется OutBox MassTransit
    /// </summary>
    public static IServiceCollection UseOutBoxServiceBus<TOutBoxContext>(
        this IServiceCollection services)
        where TOutBoxContext : DbContext
    {
        services
            .AddTransient(serviceProvider => new CrossDomainOutBox
            {
                DbContext = serviceProvider.GetService<TOutBoxContext>()
            });

        return services;
    }

    /// <summary>
    /// Подключать если используется OutBox MassTransit
    /// </summary>
    public static IServiceCollection AddCrossDomainMediator(
        this IServiceCollection services)
    {
        services
            .AddScoped<ICrossDomainMediator, CrossDomainMediator>();

        return services;
    }
}