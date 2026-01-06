namespace Avemepls.Domain.Workflow;

/// <summary>
/// Движок для описания workflow по переходу объекта между состояниями
/// </summary>
#pragma warning disable S1694
public abstract class WorkflowEngine<T, TState>
#pragma warning restore S1694
{
    /// <summary>
    /// Возвращает текущее состояние объекта
    /// </summary>
    public abstract Task<WorkflowState<T, TState>> GetState(T model, CancellationToken cancellationToken = default);
}