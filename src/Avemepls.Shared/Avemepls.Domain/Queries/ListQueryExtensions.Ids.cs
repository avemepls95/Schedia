namespace Avemepls.Domain.Queries;

/// <summary>
/// Extensions methods for ListQuery query
/// </summary>
public static partial class ListQueryExtensions
{
    public static ListQuery<TModel, TId> AddIds<TModel, TId>(
        this ListQuery<TModel, TId> source,
        params TId[] ids)
    {
        var nonNullIds = ids.Where(i => !Equals(i, default)).ToArray();
        if (nonNullIds.Any())
        {
            source.Ids = source.Ids != null
                ? source.Ids.Union(nonNullIds).ToArray()
                : nonNullIds;
        }

        return source;
    }

    public static ListQuery<TModel, TId> ExcludeIds<TModel, TId>(
        this ListQuery<TModel, TId> source,
        params TId[] ids)
    {
        var nonNullIds = ids.Where(i => !Equals(i, default)).ToArray();
        if (nonNullIds.Any())
        {
            source.ExcludeIds = source.ExcludeIds != null
                ? source.ExcludeIds.Union(nonNullIds).ToArray()
                : nonNullIds;
        }

        return source;
    }

    public static ListQuery<TModel, TId> AddSortByIds<TModel, TId>(
        this ListQuery<TModel, TId> source,
        params TId[] ids)
    {
        var nonNullIds = ids.Where(i => !Equals(i, default)).ToArray();

        if (nonNullIds.Any())
        {
            source.SortByIds = source.SortByIds != null
                ? source.SortByIds.Union(nonNullIds).ToArray()
                : nonNullIds;
        }

        return source;
    }
}