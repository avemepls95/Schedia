namespace Avemepls.Domain.Queries;

/// <summary>
/// Extensions for ListQuery
/// </summary>
public static partial class ListQueryExtensions
{
    /// <summary>
    /// Ignore all queryable modifiers when querying
    /// </summary>
    /// <param name="source">Source query</param>
    /// <typeparam name="TModel">Type of model</typeparam>
    /// <typeparam name="TId">Type of model's ID</typeparam>
    /// <returns>Modified list query</returns>
    public static GetEntityByIdQuery<TModel, TId> IgnoreQueryableModifiers<TModel, TId>(this GetEntityByIdQuery<TModel, TId> source)
    {
        source.IgnoreAllQueryableModifiers = true;
        return source;
    }
}