namespace Avemepls.Auditor.Core.Notifications;

/// <summary>
/// Notification about entity changed (already succeeded) event.
/// </summary>
/// <typeparam name="T">Type of entity.</typeparam>
public class EntityUpdatedNotification<T> : IEntityUpdatedNotification<T>
{
    /// <summary>
    /// Entity changed
    /// </summary>
    public T Entity { get; }

    /// <summary>
    /// Action (added, deteled etc)
    /// </summary>
    public EntityAction Action { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EntityUpdatedNotification{T}"/>.
    /// </summary>
    public EntityUpdatedNotification(T entity, EntityAction action)
    {
        Entity = entity;
        Action = action;
    }

    /// <summary>
    /// Creates notification about added entity.
    /// </summary>
    /// <param name="entity">Added entity.</param>
    public static EntityUpdatedNotification<T> CreateAdded(T entity) => new(entity, EntityAction.Added);

    /// <summary>
    /// Creates notification about modified entity.
    /// </summary>
    /// <param name="entity">Modified entity.</param>
    public static EntityUpdatedNotification<T> CreateModified(T entity) => new(entity, EntityAction.Modified);

    /// <summary>
    /// Creates notification about deleted entity.
    /// </summary>
    /// <param name="entity">Deleted entity.</param>
    public static EntityUpdatedNotification<T> CreateDeleted(T entity) => new(entity, EntityAction.Deleted);
}