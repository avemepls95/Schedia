namespace Avemepls.Mvc.StateProviders.Filter;

/// <summary>
/// Used to persist page's filter value somewhere. Default provider is query string.
/// </summary>
public interface IFilterStateProvider
{
    /// <summary>
    /// Persis filter value to storage.
    /// </summary>
    /// <param name="prefix">Filter's prefix so several filters on page can perist in page-dependent storage, for example into query-string.</param>
    /// <param name="state">Filter state.</param>
    /// <typeparam name="TState">Type of filter state.</typeparam>
    Task Persist<TState>(string? prefix, TState state);

    /// <summary>
    /// Restores filter state from storage.
    /// </summary>
    /// <returns>True is any value was restored, False otherwise.
    /// You can analyze return value to decide wether filter should be initialized with default values</returns>
    Task<bool> Restore<TState>(string? prefix, TState state);
}