using Avemepls.Core.Models;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Extensions methods for ListQuery query
/// </summary>
public static partial class ListQueryExtensions
{
    public static ListQuery<TModel> ExcludeIds<TModel>(this ListQuery<TModel> source, params Id<TModel>[] ids)
        where TModel : class
    {
        var nonNullIds = ids.Where(i => !Equals(i, null)).ToArray();

        if (nonNullIds.Any())
        {
            source.ExcludeIds = source.ExcludeIds != null
                ? source.ExcludeIds.Union(nonNullIds).ToArray()
                : nonNullIds;
        }

        return source;
    }

    public static ListQuery<TModel> AddSortByIds<TModel>(
        this ListQuery<TModel> source,
        params Id<TModel>[] ids)
        where TModel : class
    {
        var nonNullIds = ids.Where(i => !Equals(i.Value, null) && !Equals(i.Value, 0)).ToArray();

        if (nonNullIds.Any())
        {
            source.SortByIds = source.SortByIds != null
                ? source.SortByIds.Union(nonNullIds).ToArray()
                : nonNullIds;
        }

        return source;
    }
}