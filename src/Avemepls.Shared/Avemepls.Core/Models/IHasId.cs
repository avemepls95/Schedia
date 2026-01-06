namespace Avemepls.Core.Models;

public interface IHasId<TEntity>
    where TEntity : class
{
    public Id<TEntity> Id { get; set; }
}