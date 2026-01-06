namespace Avemepls.Domain.Workflow;

/// <summary>
/// Переход между статусами объекта
/// </summary>
public class WorkflowTransition<T, TState>
{
    /// <summary>
    /// Уникальный идентификатор перехода
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Название перехода
    /// </summary>
    public string Name { get; }

    public WorkflowState<T, TState> CurrentState { get; }

    /// <summary>
    /// Целевой статус, в который данный переход помещает сущность
    /// </summary>
    public WorkflowState<T, TState> TargetState { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkflowTransition{T,TState}"/>.
    /// </summary>
    public WorkflowTransition(string id, string name, WorkflowState<T, TState> targetState)
    {
        Id = id;
        Name = name;
        TargetState = targetState;
    }

    /// <summary>
    /// Применяет данный переход, переводит сущность в целевое состояние
    /// </summary>
    public virtual Task Apply(T model, CancellationToken cancellationToken = default)
    {
        return TargetState.MoveIn(model, cancellationToken);
    }
}