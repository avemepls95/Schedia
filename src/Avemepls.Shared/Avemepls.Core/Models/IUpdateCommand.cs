namespace Avemepls.Core.Models;

/// <summary>
/// Command to update entity
/// </summary>
public interface IUpdateCommand<TEntity>
    where TEntity : class, IHasId<TEntity>
{
    public Id<TEntity> Id { get; set; }
}