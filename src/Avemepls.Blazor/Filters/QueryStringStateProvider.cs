using System.Net;

using Avemepls.Mvc.StateProviders.Filter;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Avemepls.Blazor.Filters;

public class QueryStringStateProvider(NavigationManager navigationManager) : IFilterStateProvider
{
    public Task Persist<TState>(string? prefix, TState state)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var parameters = ObjectKvpJsonPersister.Persist(prefix, state)
            .Select(kv => new KeyValuePair<string, StringValues>(kv.Key, WebUtility.UrlEncode(kv.Value)));

        foreach (var parameter in parameters)
        {
            query[parameter.Key] = parameter.Value;
        }

        var url = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), query);
        if (!string.Equals(uri.ToString(), url))
        {
            navigationManager.NavigateTo(url, false, true);
        }

        return Task.CompletedTask;
    }

    public Task<bool> Restore<TState>(string? prefix, TState state)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var res = ObjectKvpJsonPersister.Restore(prefix, state, query.Select(kv => new KeyValuePair<string, string?>(kv.Key, WebUtility.UrlDecode(kv.Value.ToString()))));
        return Task.FromResult(res);
    }
}