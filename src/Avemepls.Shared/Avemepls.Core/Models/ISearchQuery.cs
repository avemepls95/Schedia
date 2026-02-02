namespace Avemepls.Core.Models;

public interface ISearchQuery<TId> : IListQuery<TId>
{
    /// <summary>
    /// Search query (term)
    /// </summary>
    string? Query { get; set; }
}