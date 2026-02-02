using System.Linq.Expressions;

using Avemepls.Core.DataAccess.Extensions;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Extension methods for ListQuery
/// </summary>
public static partial class ListQueryExtensions
{
    private static string GetPropertyPath<T, TResult>(Expression<Func<T, TResult>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return GetPropertyPathFromExpression(expression.Body);
    }

    private static string GetPropertyPathFromExpression(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression is MemberExpression memberExpression)
        {
            // Handle MemberExpression (property or field)
            string parentPath = GetPropertyPathFromExpression(memberExpression.Expression!);
            string memberName = memberExpression.Member.Name;

            return string.IsNullOrEmpty(parentPath)
                ? memberName
                : $"{parentPath}.{memberName}";
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            // Handle UnaryExpression (e.g., type conversions)
            return GetPropertyPathFromExpression(unaryExpression.Operand);
        }
        else if (expression is ParameterExpression)
        {
            // Handle ParameterExpression (simple lambda parameter)
            return string.Empty;
        }
        else
        {
            throw new ArgumentException("Unsupported expression type: " + expression.GetType().Name,
                                        nameof(expression));
        }
    }

    public static ListQuery<TModel, TId> OrderBy<TModel, TKey, TId>(
        this ListQuery<TModel, TId> source,
        Expression<Func<TModel, TKey>> lambda,
        bool isDescending)
    {
        var propertyName = GetPropertyPath(lambda);

        var sortInfo = isDescending
            ? "-" + propertyName
            : propertyName;

        source.OrderBy = source.OrderBy == null
            ? new[] { sortInfo }
            : source.OrderBy.Concat(new[] { sortInfo }).ToArray();

        return source;
    }

    public static ListQuery<TModel, TId> OrderBy<TModel, TKey, TId>(
        this ListQuery<TModel, TId> source,
        Expression<Func<TModel, TKey>> keySelector)
    {
        return source.OrderBy(keySelector, false);
    }

    public static ListQuery<TModel, TId> OrderBy<TModel, TId>(
        this ListQuery<TModel, TId> source,
        string propertyName,
        bool isDescending)
    {
        var sortInfo = isDescending
            ? "-" + propertyName
            : propertyName;

        source.OrderBy = source.OrderBy == null
            ? new[] { sortInfo }
            : source.OrderBy.Concat(new[] { sortInfo }).ToArray();

        return source;
    }

    public static ListQuery<TModel, TId> OrderBy<TModel, TId>(this ListQuery<TModel, TId> source, string propertyName)
    {
        return source.OrderBy(propertyName, false);
    }

    public static ListQuery<TModel, TId> OrderByDescending<TModel, TKey, TId>(
        this ListQuery<TModel, TId> source,
        Expression<Func<TModel, TKey>> keySelector)
    {
        return source.OrderBy(keySelector, true);
    }

    public static ListQuery<TModel, TId> OrderByDescending<TModel, TId>(
        this ListQuery<TModel, TId> source,
        string propertyName)
    {
        return source.OrderBy(propertyName, true);
    }

    /// <summary>
    /// Add sort expressions to <paramref name="query"/>
    /// </summary>
    /// <param name="query">Queryable</param>
    /// <param name="orderBy">
    ///     Fields to sort by. The order will be applied according to the order of the elements in the collection. To
    ///     sort in descending order, use "-" at the beginning of the property name. For example: "name,-createdDate"
    /// </param>
    /// <param name="isNestedSort">An indicator indicating whether the sorting has already been applied previously.</param>
    /// <typeparam name="T">Entity</typeparam>
    /// <returns>Modified queryable</returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, string[]? orderBy, bool isNestedSort = false)
    {
        if (orderBy?.Any() != true)
        {
            return query;
        }

        var resultQuery = query;

        foreach (var sortableField in orderBy)
        {
            var (propertyName, descending) = sortableField.NormalizeSortPath();
            resultQuery = resultQuery.OrderByProperty(propertyName, descending, isNestedSort);
            isNestedSort = true;
        }

        return resultQuery;
    }

    public static ListQuery<TModel, TId> ClearSort<TModel, TId>(this ListQuery<TModel, TId> source)
    {
        source.OrderBy = null;

        return source;
    }
}