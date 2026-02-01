using Avemepls.ServiceBus;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Schedia.DataAccess;
using Schedia.Web.Base.Configs;

namespace Schedia.Web.Base;

// #PoIgnore#
public static class ProgramExtensionsMassTransit
{
    public static IServiceCollection AddSchediaMassTransit(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCrossDomainMediator();

        var schediaAsssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().Name!.StartsWith(nameof(Schedia), StringComparison.InvariantCulture))
            .ToArray();

        var configSection = configuration.GetSection("ServiceBus").Get<ServiceBusConfig>()!;

        services.UseOutBoxServiceBus<SchediaDbContext>();
        services.AddMassTransitConsumerLogging();

        services.AddMassTransit(options =>
        {
            options.SetKebabCaseEndpointNameFormatter();
            options.DisableUsageTelemetry();

            options.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseRawJsonSerializer();
                cfg.UseRawJsonDeserializer();

                cfg.Host(configSection.Host,
                    configSection.VirtualHost,
                    mqHostConfigurator =>
                    {
                        mqHostConfigurator.Username(configSection.UserName);
                        mqHostConfigurator.Password(configSection.Password);
                    });

                cfg.ConfigurePublish(publishConfig =>
                {
                    publishConfig.AddScopeContext();
                });

                cfg.RegisterCrossDomainHandlers(context, schediaAsssemblies);
                cfg.UseConsumerLogging(context);

                cfg.ConfigureEndpoints(context);
                cfg.ConcurrentMessageLimit = 5;
                cfg.PrefetchCount = 25;

                cfg.UseMessageRetry(retryConfig =>
                {
                    // 100, 400, 700 и 1000 ms
                    retryConfig.Incremental(4, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(300));
                });
            });

            options.AddEntityFrameworkOutbox<SchediaDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox(x => x.MessageDeliveryLimit = 5);
                o.LockStatementProvider = new PostgresLockStatementProvider();
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.QueryMessageLimit = 100;
            });
        });

        return services;
    }
}