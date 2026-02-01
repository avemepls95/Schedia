namespace Avemepls.Core.Models;

public interface ISearchQuery<TModel> : IListQuery<TModel>
    where TModel : class
{
    /// <summary>
    /// Search query (term)
    /// </summary>
    string? Query { get; set; }
}