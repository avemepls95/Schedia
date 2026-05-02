using System.Reflection;

using Avemepls.Core.Extensions;
using Avemepls.Core.Models;
using Avemepls.ServiceBus.Filters;
using Avemepls.ServiceBus.Helpers;
using Avemepls.ServiceBus.Observers;

using MassTransit;
using MassTransit.Configuration;

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

    public static IServiceCollection AddConsumers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var registerConsumerWithDefinitionMethod = typeof(DependencyInjectionConsumerRegistrationExtensions).GetGenericMethod("RegisterConsumer",
            typeof(IServiceCollection),
            typeof(Type)
        );

        var registerConsumerMethod = typeof(DependencyInjectionConsumerRegistrationExtensions).GetGenericMethod("RegisterConsumer",
            typeof(IServiceCollection)
        );

        var consumerAndDefinitions = assemblies
            .SelectMany(x => x.DefinedTypes)
            .Where(e =>
                e is { IsClass: true, IsAbstract: false }
                && (e.IsAssignableToGenericType(typeof(IConsumer<>)) || e.IsAssignableToGenericType(typeof(IConsumerDefinition<>)))
            )
            .ToArray();

        var definitions = consumerAndDefinitions
            .Where(e => e.IsAssignableToGenericType(typeof(IConsumerDefinition<>)))
            .ToDictionary(x =>
                x.GetInterfaces()
                    .First(w => w.IsGenericType && w.GetGenericTypeDefinition() == typeof(IConsumerDefinition<>))
                    .GetGenericArguments()[0]
            );

        var consumers = consumerAndDefinitions.Where(e => e.IsAssignableToGenericType(typeof(IConsumer<>)));

        foreach (var consumer in consumers)
        {
            if (definitions.TryGetValue(consumer, out var definition))
            {
                var genericMethod = registerConsumerWithDefinitionMethod!.MakeGenericMethod(consumer);
                genericMethod.Invoke(null, [services, definition!]);
            }
            else
            {
                var genericMethod = registerConsumerMethod!.MakeGenericMethod(consumer);
                genericMethod.Invoke(null, [services]);
            }
        }

        return services;
    }

    private static MethodInfo? GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
    {
        var methods = type.GetMethods();

        foreach (var method in methods.Where(m => m.Name == name))
        {
            var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
            {
                return method;
            }
        }

        return null;
    }

    private sealed class SimpleTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type? x, Type? y)
            => x != null && y != null && x.Assembly == y.Assembly && x.Namespace == y.Namespace && x.Name == y.Name;

        public int GetHashCode(Type obj) => throw new NotImplementedException();
    }
}