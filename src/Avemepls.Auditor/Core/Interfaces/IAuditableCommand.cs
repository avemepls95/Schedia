namespace Avemepls.Auditor.Core.Interfaces;

/// <summary>
/// CQRS-command that intended to be logged with special auditing details.
/// Command should implement this interface to be processed via pipeline of CommandProcessingAuditors.
/// </summary>
public interface IAuditableCommand
{
}