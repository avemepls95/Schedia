using Avemepls.Auditor.Commands;

namespace Avemepls.Auditor.Interfaces;

/// <summary>
/// Command auditor implementation interface
/// </summary>
/// <typeparam name="TCommand">Type of command this auditor is processing</typeparam>
public interface ICommandProcessingAuditor<in TCommand>
{
    /// <summary>
    /// Called before processing the command
    /// </summary>
    /// <param name="auditScope">Scope for audit</param>
    /// <param name="command">Instance of processing command</param>
    /// ///
    /// <param name="context">Context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask PreProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called after processing the command
    /// </summary>
    /// <param name="auditScope">Scope for audit</param>
    /// <param name="command">Instance of processing command</param>
    /// <param name="context">Context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask PostProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken);
}

/// <summary>
/// Command auditor implementation interface
/// </summary>
/// <typeparam name="TCommand">Type of command this auditor is processing</typeparam>
/// <typeparam name="TResult">Type of command execution result</typeparam>
public interface ICommandProcessingAuditor<in TCommand, in TResult>
{
    /// <summary>
    /// Called before processing the command
    /// </summary>
    /// <param name="auditScope">Scope for audit</param>
    /// <param name="command">Instance of processing command</param>
    /// ///
    /// <param name="context">Context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask PreProcess(
        AuditScope auditScope,
        TCommand command,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called after processing the command
    /// </summary>
    /// <param name="auditScope">Scope for audit</param>
    /// <param name="command">Instance of processing command</param>
    /// <param name="response">Result, returned from execution of command</param>
    /// <param name="context">Context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask PostProcess(
        AuditScope auditScope,
        TCommand command,
        TResult? response,
        CommandProcessingAuditorContext context,
        CancellationToken cancellationToken);
}