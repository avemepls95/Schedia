using Avemepls.Core.Models;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Search by query text.
/// </summary>
public abstract class SearchQuery<TModel>(bool includeDeleted = false, bool includeNonActive = false)
    : SearchQuery<TModel, int>(includeDeleted, includeNonActive)
{
}

/// <summary>
/// Search by query text.
/// </summary>
#pragma warning disable SA1402
public abstract class SearchQuery<TModel, TId>(bool includeDeleted = false, bool includeNonActive = false)
    : ListQuery<TModel, TId>(includeDeleted, includeNonActive), ISearchQuery<TId>
#pragma warning restore SA1402
{
    /// <summary>
    /// Search query (term)
    /// </summary>
    public string? Query { get; set; }
}