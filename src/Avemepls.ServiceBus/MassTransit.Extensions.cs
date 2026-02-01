using System.Reflection;

using Avemepls.Core.Extensions;
using Avemepls.Core.Models;
using Avemepls.ServiceBus.Filters;
using Avemepls.ServiceBus.Helpers;
using Avemepls.ServiceBus.Observers;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.ServiceBus;

public static class MassTransitExtensions
{
    /// <summary>
    /// Регистрирует обработчики запросов и очереди для них на основе контракта запроса
    /// </summary>
    public static void RegisterCrossDomainHandlers(
        this IBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        params Assembly[] assemblies)
    {
        var consumerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                            .Where(i => i.IsAssignableToGenericType(typeof(IConsumer<>)))
                            .Select(i => new { ConsumerType = t, MessageType = i.GetGenericArguments()[0] }))
            .Where(x => typeof(ICrossDomainRequestBase).IsAssignableFrom(x.MessageType))
            .ToList();

        foreach (var consumerInfo in consumerTypes)
        {
            var endpointName = EndpointHelper.GenerateEndpointName(consumerInfo.MessageType);

            cfg.ReceiveEndpoint(endpointName,
                                ep =>
                                {
                                    ep.ConfigureConsumer(context, consumerInfo.ConsumerType);
                                });
        }
    }

    /// <summary>
    /// Регистрирует ConsumerLoggingObserver для логирования всех консьюмеров
    /// </summary>
    public static IServiceCollection AddMassTransitConsumerLogging(this IServiceCollection services)
    {
        services.AddSingleton<ConsumerLoggingObserver>();
        return services;
    }

    /// <summary>
    /// Подключает ConsumerLoggingObserver к шине для логирования всех консьюмеров
    /// </summary>
    public static void UseConsumerLogging(this IBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        var observer = context.GetRequiredService<ConsumerLoggingObserver>();
        cfg.ConnectConsumeObserver(observer);
    }

    /// <summary>
    /// Add <see cref="ScopeContextPublishFilter"/> to publish pipe
    /// </summary>
    public static void AddScopeContext(this IPublishPipeConfigurator configurator) =>
        configurator.UseFilter(new ScopeContextPublishFilter());
}