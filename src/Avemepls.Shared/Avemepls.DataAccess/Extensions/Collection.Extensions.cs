using Avemepls.Core.Models;

using EntityFrameworkCore.Projectables;

namespace Avemepls.Core.DataAccess.Extensions;

public static class CollectionExtensions
{
    [Projectable]
    public static IEnumerable<TEntity> NonDeleted<TEntity>(this ICollection<TEntity> collection)
        where TEntity : IHasDateDeleted =>
        collection.Where(x => x.DateDeleted == null);

    [Projectable]
    public static IEnumerable<TEntity> Active<TEntity>(this ICollection<TEntity> collection)
        where TEntity : IHasIsActive =>
        collection.Where(x => x.IsActive);

    [Projectable]
    public static IEnumerable<TEntity> Available<TEntity>(this ICollection<TEntity> collection)
        where TEntity : IHasIsActive, IHasDateDeleted =>
        collection.Where(x => x.IsActive && x.DateDeleted == null);
}