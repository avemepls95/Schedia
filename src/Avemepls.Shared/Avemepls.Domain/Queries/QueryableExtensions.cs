using System.Collections.Concurrent;

using Avemepls.Core.Models;

namespace Avemepls.Domain.Queries;

internal static class QueryableExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> EntityHasDateDeletedCache = new();
    private static readonly ConcurrentDictionary<Type, bool> EntityHasIsActiveCache = new();
    private static readonly ConcurrentDictionary<Type, bool> EntityHasIsActiveNullableCache = new();

    /// <summary>
    /// Добавляет условие на не удаленность, если сущность реализует <see cref="IHasDateDeleted"/>. В ином случае
    /// ничего не происходит.
    /// </summary>
    public static IQueryable<TEntity> WhereIsNotDeleted<TEntity>(this IQueryable<TEntity> queryable)
    {
        var entityType = typeof(TEntity);

        var hasDateDeleted = EntityHasDateDeletedCache.GetOrAdd(
            entityType,
            type => type.IsAssignableTo(typeof(IHasDateDeleted)));

        return hasDateDeleted
            ? queryable.Where(e => ((IHasDateDeleted)e!).DateDeleted == null)
            : queryable;
    }

    /// <summary>
    /// Добавляет условие на активность, если сущность реализует <see cref="IHasIsActive"/>
    /// или <see cref="IHasIsActiveNullable"/>. В ином случае ничего не происходит.
    /// </summary>
    public static IQueryable<TEntity> WhereIsActive<TEntity>(this IQueryable<TEntity> queryable)
    {
        var entityType = typeof(TEntity);

        var hasIsActive = EntityHasIsActiveCache.GetOrAdd(
            entityType,
            type => type.IsAssignableTo(typeof(IHasIsActive)));

        if (hasIsActive)
        {
            return queryable.Where(e => ((IHasIsActive)e!).IsActive);
        }

        var hasIsActiveNullable = EntityHasIsActiveNullableCache.GetOrAdd(
            entityType,
            type => type.IsAssignableTo(typeof(IHasIsActiveNullable)));

        return hasIsActiveNullable
            ? queryable.Where(e => ((IHasIsActiveNullable)e!).IsActive == true)
            : queryable;
    }
}