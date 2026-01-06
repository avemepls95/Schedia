using MediatR;

namespace Avemepls.Auditor.Core.Notifications;

/// <summary>
/// Fires when entity is updating (just before persistence)
/// </summary>
/// <typeparam name="T">Type of entity</typeparam>
public interface IEntityUpdatingNotification<out T> : INotification
{
    T Entity { get; }

    EntityAction Action { get; }
}