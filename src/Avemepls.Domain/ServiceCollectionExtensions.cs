using System.Reflection;

using Avemepls.Domain.Filters;
using Avemepls.Domain.Security;

using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Domain;

/// <summary>
/// Extensions for the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static Type? GetGenericInterface(this Type type, Type interfaceType)
    {
        return Array.Find(
            type.GetInterfaces(),
            theInterface =>
                theInterface.IsGenericType &&
                theInterface.GetGenericTypeDefinition() == interfaceType);
    }

    /// <summary>
    /// Добавляет все реализации обобщенного интерфейса.
    /// </summary>
    public static IServiceCollection AddGenericInterfaceImplementations(
        this IServiceCollection services,
        Assembly assembly,
        Type genericInterfaceType,
        ServiceLifetime lifetime)
    {
        var types = assembly.DefinedTypes.Where(e => e.IsClass && !e.IsAbstract && !e.IsGenericType);

        foreach (var type in types)
        {
            var queryableModifier = type.GetGenericInterface(genericInterfaceType);

            if (queryableModifier == null)
            {
                continue;
            }

            var descriptor = new ServiceDescriptor(queryableModifier, type, lifetime);

            if (services.Contains(descriptor))
            {
                continue;
            }

            services.Add(descriptor);
        }

        return services;
    }

    /// <summary>
    /// Добавить модификаторы запросов.
    /// </summary>
    public static IServiceCollection AddQueryableModifiers(this IServiceCollection services, Assembly assembly) =>
        services.AddGenericInterfaceImplementations(
            assembly,
            typeof(IQueryableModifier<>),
            ServiceLifetime.Transient);

    /// <summary>
    /// Добавить ограничения.
    /// </summary>
    public static IServiceCollection AddPermissionEvaluators(this IServiceCollection services, Assembly assembly) =>
        services.AddGenericInterfaceImplementations(
            assembly,
            typeof(IPermissionEvaluator<>),
            ServiceLifetime.Transient);
}