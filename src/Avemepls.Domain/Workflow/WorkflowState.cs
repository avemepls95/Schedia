namespace Avemepls.Domain.Workflow;

/// <summary>
/// Статус объекта
/// </summary>
public abstract class WorkflowState<TModel, TState>
{
    /// <summary>
    /// Уникальный идентификатор статуса
    /// </summary>
    public abstract TState Id { get; }

    /// <summary>
    /// Возвращает перечень возможных переходов из данного состояния
    /// </summary>
    public abstract Task<ICollection<WorkflowTransition<TModel, TState>>> GetTransitions(TModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Помещает сущность в текущее состояние
    /// </summary>
    public abstract Task MoveIn(TModel model, CancellationToken cancellationToken);

    protected static Task<ICollection<WorkflowTransition<TModel, TState>>> NoTransitions
        => Task.FromResult<ICollection<WorkflowTransition<TModel, TState>>>(Array.Empty<WorkflowTransition<TModel, TState>>());

    public virtual string GetTransitionId(TState state)
    {
        return $"{Id} -> {state}";
    }
}