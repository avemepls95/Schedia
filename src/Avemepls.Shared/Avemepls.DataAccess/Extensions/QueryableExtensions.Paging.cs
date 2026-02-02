using Avemepls.Core.Models;

namespace Avemepls.Core.DataAccess.Extensions;

/// <summary>
/// Common extensions for IQueryable
/// </summary>
public static partial class QueryableExtensions
{
    public static IQueryable<TEntity> Paging<TEntity>(this IQueryable<TEntity> query, ILimitQuery? limiter)
    {
        if (limiter == null)
            return query;

        if (limiter.Offset != 0)
            query = query.Skip(limiter.Offset);

        if (limiter.Limit != null)
            query = query.Take(limiter.Limit.Value);

        return query;
    }

    public static IQueryable<TEntity> Paging<TEntity, TId>(this IQueryable<TEntity> query, ILimitQuery? limiter)
        where TEntity : IHasId<TId>
    {
        if (limiter == null)
            return query;

        if ((limiter.Offset != 0 || limiter.Limit != null)
            && query.Expression.Type != typeof(IOrderedQueryable<TEntity>))
            query = query.OrderBy(q => q.Id);

        if (limiter.Offset != 0)
            query = query.Skip(limiter.Offset);

        if (limiter.Limit != null)
            query = query.Take(limiter.Limit.Value);

        return query;
    }
}