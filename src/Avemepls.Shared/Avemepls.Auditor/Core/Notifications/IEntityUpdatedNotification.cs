using MediatR;

namespace Avemepls.Auditor.Core.Notifications;

/// <summary>
/// Fires after entity persistence
/// </summary>
/// <typeparam name="T">Type of entity</typeparam>
public interface IEntityUpdatedNotification<out T> : INotification
{
    T Entity { get; }

    EntityAction Action { get; }
}

/// <summary>
/// Action performed on entity
/// </summary>
public enum EntityAction
{
    /// <summary>
    /// Entity added
    /// </summary>
    Added = 0,

    /// <summary>
    /// Entity modified
    /// </summary>
    Modified = 1,

    /// <summary>
    /// Entity deleted (incl. soft deletion)
    /// </summary>
    Deleted = 2
}