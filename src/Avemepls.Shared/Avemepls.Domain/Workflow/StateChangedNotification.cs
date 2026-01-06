using MediatR;

namespace Avemepls.Domain.Workflow;

/// <summary>
/// Уведомление об изменении состояния.
/// </summary>
/// <typeparam name="TEntity">Тип сущности.</typeparam>
/// <typeparam name="TState">Тип состояния.</typeparam>
public class StateChangedNotification<TEntity, TState>(TEntity entity, TState previousState, TState newState) : INotification
{
    /// <summary>
    /// Прежнее состояние.
    /// </summary>
    public TState PreviousState { get; set; } = previousState;

    /// <summary>
    /// Новое состояние.
    /// </summary>
    public TState NewState { get; set; } = newState;

    /// <summary>
    /// Сущность.
    /// </summary>
    public TEntity Entity { get; set; } = entity;
}