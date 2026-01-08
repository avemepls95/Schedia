using Avemepls.Core.Models;

namespace Avemepls.Core.DataAccess.Extensions;

/// <summary>
/// Common extensions for IQueryable
/// </summary>
public static partial class QueryableExtensions
{
    public static IQueryable<TEntity> NonDeleted<TEntity>(this IQueryable<TEntity> query)
        where TEntity : IHasDateDeleted
    {
        return query.Where(e => e.DateDeleted == null);
    }

    public static IQueryable<TEntity> Active<TEntity>(this IQueryable<TEntity> query)
        where TEntity : IHasIsActive
    {
        return query.Where(e => e.IsActive);
    }

    public static bool Active<TEntity>(this TEntity entity)
        where TEntity : IHasIsActive =>
        entity.IsActive;

    public static IQueryable<TEntity> Available<TEntity>(this IQueryable<TEntity> collection)
        where TEntity : IHasIsActive, IHasDateDeleted =>
        collection.Where(x => x.IsActive && x.DateDeleted == null);

    public static bool Available<TEntity>(this TEntity entity)
        where TEntity : IHasIsActive, IHasDateDeleted =>
        entity.DateDeleted == null && entity.IsActive;
}