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

        switch (expression)
        {
            case MemberExpression memberExpression:
            {
                // Handle MemberExpression (property or field)
                string parentPath = GetPropertyPathFromExpression(memberExpression.Expression!);
                string memberName = memberExpression.Member.Name;

                return string.IsNullOrEmpty(parentPath)
                    ? memberName
                    : $"{parentPath}.{memberName}";
            }
            case UnaryExpression unaryExpression:
                // Handle UnaryExpression (e.g., type conversions)
                return GetPropertyPathFromExpression(unaryExpression.Operand);
            case ParameterExpression:
                // Handle ParameterExpression (simple lambda parameter)
                return string.Empty;
            default:
                throw new ArgumentException("Unsupported expression type: " + expression.GetType().Name,
                    nameof(expression));
        }
    }

    public static ListQuery<TModel> OrderBy<TModel, TKey>(
        this ListQuery<TModel> source,
        Expression<Func<TModel, TKey>> lambda,
        bool isDescending)
        where TModel : class
    {
        var propertyName = GetPropertyPath(lambda);

        var sortInfo = isDescending
            ? "-" + propertyName
            : propertyName;

        source.OrderBy = source.OrderBy == null
            ? [sortInfo]
            : source.OrderBy.Concat([sortInfo]).ToArray();

        return source;
    }

    public static ListQuery<TModel> OrderBy<TModel, TKey>(
        this ListQuery<TModel> source,
        Expression<Func<TModel, TKey>> keySelector)
        where TModel : class
    {
        return source.OrderBy(keySelector, false);
    }

    public static ListQuery<TModel> OrderBy<TModel>(
        this ListQuery<TModel> source,
        string propertyName,
        bool isDescending)
        where TModel : class
    {
        var sortInfo = isDescending
            ? "-" + propertyName
            : propertyName;

        source.OrderBy = source.OrderBy == null
            ? [sortInfo]
            : source.OrderBy.Concat([sortInfo]).ToArray();

        return source;
    }

    public static ListQuery<TModel> OrderBy<TModel>(this ListQuery<TModel> source, string propertyName)
        where TModel : class
    {
        return source.OrderBy(propertyName, false);
    }

    public static ListQuery<TModel> OrderByDescending<TModel, TKey>(
        this ListQuery<TModel> source,
        Expression<Func<TModel, TKey>> keySelector)
        where TModel : class
    {
        return source.OrderBy(keySelector, true);
    }

    public static ListQuery<TModel> OrderByDescending<TModel>(
        this ListQuery<TModel> source,
        string propertyName)
        where TModel : class
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

    public static ListQuery<TModel> ClearSort<TModel>(this ListQuery<TModel> source)
        where TModel : class
    {
        source.OrderBy = null;

        return source;
    }
}