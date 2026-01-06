using Avemepls.Domain.Queries;

namespace Avemepls.Blazor.Components.Forms;

public class QuerySelect<TItem, TItemValue, TQuery> : QuerySelectBase<TItem, TItemValue, TQuery>
    where TQuery : SearchQuery<TItem>, new()
    where TItem : class
{
}