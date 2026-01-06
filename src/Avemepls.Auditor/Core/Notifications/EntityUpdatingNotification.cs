namespace Avemepls.Auditor.Core.Notifications;

/// <summary>
/// Notification about entity changing event.
/// </summary>
public class EntityUpdatingNotification<T> : IEntityUpdatingNotification<T>
{
    /// <summary>
    /// Entity.
    /// </summary>
    public T Entity { get; }

    /// <summary>
    /// Action type.
    /// </summary>
    public EntityAction Action { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="EntityUpdatingNotification{T}"/>.
    /// </summary>
    public EntityUpdatingNotification(T entity, EntityAction action)
    {
        Entity = entity;
        Action = action;
    }

    /// <summary>
    /// Creates notification about added entity.
    /// </summary>
    /// <param name="entity">Added entity.</param>
    public static EntityUpdatingNotification<T> CreateAdded(T entity) => new(entity, EntityAction.Added);

    /// <summary>
    /// Creates notification about modified entity.
    /// </summary>
    /// <param name="entity">Modified entity.</param>
    public static EntityUpdatingNotification<T> CreateModified(T entity) => new(entity, EntityAction.Modified);

    /// <summary>
    /// Creates notification about deleted entity.
    /// </summary>
    /// <param name="entity">Deleted entity.</param>
    public static EntityUpdatingNotification<T> CreateDeleted(T entity) => new(entity, EntityAction.Deleted);
}