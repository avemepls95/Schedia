using System.Diagnostics;

namespace Avemepls.Core.Models;

[DebuggerDisplay("{Count} result(s)")]
public class PagedResponse<TModel> : IPagedResponse<TModel>
{
    public TModel[] Results { get; set; }

    public long Count { get; set; }

    public PagedResponse(TModel[] results, long count)
    {
        Results = results;
        Count = count;
    }

    public PagedResponse()
    {
    }
}