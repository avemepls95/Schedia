using Avemepls.Core.Models;

using EntityFrameworkCore.Projectables;

namespace Avemepls.Core.DataAccess.Extensions;

public static class EntityExtensions
{
    [Projectable]
    public static bool NonDeleted<TEntity>(this TEntity entity)
        where TEntity : IHasDateDeleted =>
        entity.DateDeleted == null;

    [Projectable]
    public static bool Active<TEntity>(this TEntity entity)
        where TEntity : IHasIsActive =>
        entity.IsActive;

    [Projectable]
    public static bool Available<TEntity>(this TEntity entity)
        where TEntity : IHasIsActive, IHasDateDeleted =>
        entity.IsActive && entity.DateDeleted == null;
}