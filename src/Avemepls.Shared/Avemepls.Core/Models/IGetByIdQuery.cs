namespace Avemepls.Core.Models;

public interface IGetByIdQuery<TEntity>
    where TEntity : class
{
    public Id<TEntity> Id { get; set; }
}