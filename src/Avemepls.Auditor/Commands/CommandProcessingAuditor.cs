using Avemepls.Auditor.Core.Interfaces;
using Avemepls.Auditor.Interfaces;

namespace Avemepls.Auditor.Commands;

/// <summary>
/// Base abstract class to implement some command auditor
/// </summary>
/// <typeparam name="TCommand">Type of command this auditor is processing</typeparam>
public abstract class CommandProcessingAuditor<TCommand> : ICommandProcessingAuditor<TCommand>
    where TCommand : IAuditableCommand
{
    /// <inheritdoc cref="ICommandProcessingAuditor{TCommand,TResponse}"/>
    public virtual ValueTask PreProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="ICommandProcessingAuditor{TCommand,TResponse}"/>
    public virtual ValueTask PostProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Base abstract class to implement some command auditor
/// </summary>
/// <typeparam name="TCommand">Type of command this auditor is processing</typeparam>
/// <typeparam name="TResponse">Type of command execution result</typeparam>
#pragma warning disable SA1402
public abstract class CommandProcessingAuditor<TCommand, TResponse> : ICommandProcessingAuditor<TCommand, TResponse>
#pragma warning restore SA1402
    where TCommand : IAuditableCommand
{
    /// <inheritdoc cref="ICommandProcessingAuditor{TCommand,TResponse}"/>
    public virtual ValueTask PreProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="ICommandProcessingAuditor{TCommand,TResponse}"/>
    public virtual ValueTask PostProcess(
        AuditScope auditScope,
        TCommand command,
        TResponse? response,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}