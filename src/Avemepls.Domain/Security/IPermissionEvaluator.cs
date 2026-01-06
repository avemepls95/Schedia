namespace Avemepls.Domain.Security;

/// <summary>
/// CRUD-restriction policy
/// </summary>
public interface IPermissionEvaluator<in TEntity>
    where TEntity : class
{
    /// <summary>
    /// Checks if entity can be accessed for read
    /// </summary>
    Task<bool> CanRead(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if entity can be added
    /// </summary>
    Task<bool> CanAdd(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if user can update entity
    /// </summary>
    Task<bool> CanUpdate(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if user can delete entity
    /// </summary>
    Task<bool> CanDelete(TEntity entity, CancellationToken cancellationToken);
}