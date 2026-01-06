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
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <typeparam name="TModifier">Type of modifier to be ignored</typeparam>
    /// <returns>Modified list query</returns>
    public static ListQuery<TModel, TResult> IgnoreQueryableModifier<TModel, TResult, TModifier>(this ListQuery<TModel, TResult> source)
        where TResult : PagedResponse<TModel>, new()
        where TModel : class
    {
        source.IgnoreQueryableModifiers ??= [];
        source.IgnoreQueryableModifiers.Add(typeof(TModifier));
        return source;
    }

    /// <summary>
    /// Ignores all queryable modifiers on select
    /// </summary>
    public static ListQuery<TModel, TResult> IgnoreQueryableModifiers<TModel, TResult>(this ListQuery<TModel, TResult> source)
        where TResult : PagedResponse<TModel>, new()
        where TModel : class
    {
        source.IgnoreAllQueryableModifiers = true;
        return source;
    }
}