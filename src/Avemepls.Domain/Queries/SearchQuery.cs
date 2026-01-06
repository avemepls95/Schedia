namespace Avemepls.Domain.Queries;

/// <summary>
/// Search by query text.
/// </summary>
#pragma warning disable SA1402
public abstract class SearchQuery<TModel>(bool includeDeleted = false, bool includeNonActive = false)
    : ListQuery<TModel>(includeDeleted, includeNonActive)
    where TModel : class
#pragma warning restore SA1402
{
    /// <summary>
    /// Search query (term)
    /// </summary>
    public string? Query { get; set; }
}