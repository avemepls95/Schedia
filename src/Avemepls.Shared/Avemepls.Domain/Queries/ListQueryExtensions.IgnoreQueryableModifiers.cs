using Avemepls.Core.Models;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Extensions for ListQuery
/// </summary>
public static partial class ListQueryExtensions
{
    /// <summary>
    /// Ignore queryable modifier of certain type when querying
    /// </summary>
    /// <param name="source">Source query</param>
    /// <typeparam name="TModel">Type of model</typeparam>
    /// <typeparam name="TId">Type of model's ID</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <typeparam name="TModifier">Type of modifier to be ignored</typeparam>
    /// <returns>Modified list query</returns>
    public static ListQuery<TModel, TId, TResult> IgnoreQueryableModifier<TModel, TId, TResult, TModifier>(this ListQuery<TModel, TId, TResult> source)
        where TResult : PagedResponse<TModel>, new()
    {
        source.IgnoreQueryableModifiers ??= new HashSet<Type>();
        source.IgnoreQueryableModifiers.Add(typeof(TModifier));
        return source;
    }

    /// <summary>
    /// Ignores all queryable modifiers on select
    /// </summary>
    public static ListQuery<TModel, TId, TResult> IgnoreQueryableModifiers<TModel, TId, TResult>(this ListQuery<TModel, TId, TResult> source)
        where TResult : PagedResponse<TModel>, new()
    {
        source.IgnoreAllQueryableModifiers = true;
        return source;
    }
}