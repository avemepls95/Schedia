using System.Reflection;

using Avemepls.Core.Extensions;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Application.Core;

public static class MassTransitExtensions
{
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