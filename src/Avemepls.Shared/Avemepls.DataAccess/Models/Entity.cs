using Avemepls.Core.Models;

namespace Avemepls.Core.DataAccess.Models;

/// <summary>
/// Base class for database entities
/// </summary>
public abstract class Entity<TEntity> : IHasId<TEntity>
    where TEntity : class
{
    public Id<TEntity> Id { get; set; }
}