using System.Collections.Concurrent;

namespace Avemepls.Core.Extensions;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> Cache = new();

    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var tuple = new Tuple<Type, Type>(givenType, genericType);

        return Cache.GetOrAdd(tuple, k => k.Item1.IsAssignableToGenericTypeInternal(k.Item2));
    }

    private static bool IsAssignableToGenericTypeInternal(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var baseType = givenType.BaseType;

        return baseType != null && baseType.IsAssignableToGenericType(genericType);
    }

    /// <summary>
    /// Получает тип аргумента для заданного generic интерфейса в иерархии типов.
    /// </summary>
    /// <param name="implementationType">Тип реализации, в котором производится поиск интерфейса.</param>
    /// <param name="genericInterfaceType">Тип generic интерфейса, который необходимо найти.</param>
    /// <param name="argumentIndex">Индекс аргумента в generic интерфейсе (по умолчанию 0).</param>
    /// <returns>Тип аргумента generic интерфейса или null, если интерфейс не найден.</returns>
    public static Type? GetGenericInterfaceTypeArgument(this Type implementationType, Type genericInterfaceType, int argumentIndex = 0)
    {
        var interfaces = implementationType.GetInterfaces();

        foreach (var interfaceType in interfaces)
        {
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
            {
                return interfaceType.GetGenericArguments()[argumentIndex];
            }
        }

        return implementationType.BaseType?.GetGenericInterfaceTypeArgument(genericInterfaceType, argumentIndex);
    }
}