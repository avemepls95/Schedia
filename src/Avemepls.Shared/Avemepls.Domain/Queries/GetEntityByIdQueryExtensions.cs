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
    /// <returns>Modified list query</returns>
    public static GetEntityByIdQuery<TModel> IgnoreQueryableModifiers<TModel>(this GetEntityByIdQuery<TModel> source)
        where TModel : class
    {
        source.IgnoreAllQueryableModifiers = true;
        return source;
    }
}