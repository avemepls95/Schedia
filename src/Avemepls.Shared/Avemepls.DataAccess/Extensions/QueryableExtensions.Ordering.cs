using System.Linq.Expressions;
using System.Reflection;

using Avemepls.Core.DataAccess.Exceptions;

namespace Avemepls.Core.DataAccess.Extensions;

/// <summary>
/// Ordering extensions for IQueryable interface
/// </summary>
public static partial class QueryableExtensions
{
    private static readonly MethodInfo OrderByMethod =
        typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

    private static readonly MethodInfo OrderByDescendingMethod =
        typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByMethod =
        typeof(Queryable).GetMethods().Single(method => method.Name == "ThenBy" && method.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByDescendingMethod =
        typeof(Queryable).GetMethods().Single(method => method.Name == "ThenByDescending" && method.GetParameters().Length == 2);

    /// <summary>
    /// Order queryable ascending by string property name descending
    /// </summary>
    /// <param name="source">Source queryable</param>
    /// <param name="propertyName">Name of property to order by</param>
    /// <param name="descending">Whether to sort descending?</param>
    /// <param name="nestedOrdering">If operation is nested, "ThenBy" instead of "OrderBy" is used</param>
    /// <typeparam name="T">Type of entity</typeparam>
    /// <returns>Ordered queryable</returns>
    public static IOrderedQueryable<T> OrderByProperty<T>(
        this IQueryable<T> source,
        string propertyName,
        bool descending = false,
        bool nestedOrdering = false)
    {
        if (!IsValidPropertyPath<T>(propertyName))
            throw new OrderingPropertyNotFoundException(typeof(T), propertyName);

        var parameter = Expression.Parameter(typeof(T), "x");

        Expression orderByProperty = parameter;
        foreach (var member in propertyName.Split('.'))
        {
            orderByProperty = Expression.PropertyOrField(orderByProperty, member);
        }

        LambdaExpression lambda = Expression.Lambda(orderByProperty, parameter);

        MethodInfo genericMethod = (descending, nestedOrdering) switch
        {
            (true, true) => ThenByDescendingMethod,
            (true, false) => OrderByDescendingMethod,
            (false, true) => ThenByMethod,
            _ => OrderByMethod
        };

        object? ret = genericMethod.MakeGenericMethod(typeof(T), orderByProperty.Type).Invoke(null, [source, lambda]);

        return (IOrderedQueryable<T>)ret!;
    }

    private static bool IsValidPropertyPath<T>(string propertyPath)
    {
        Type type = typeof(T);

        foreach (var propertyName in propertyPath.Split('.'))
        {
            PropertyInfo? property = type.GetProperty(propertyName);

            if (property is null)
            {
                return false;
            }

            type = property.PropertyType;
        }

        return true;
    }

    public static (string Property, bool Descending) NormalizeSortPath(this string sortProperty)
    {
        var descending = false;

        if (sortProperty.StartsWith('-'))
        {
            descending = true;
            sortProperty = sortProperty[1..];
        }

        return (char.ToUpper(sortProperty[0]) + sortProperty[1..], descending);
    }
}