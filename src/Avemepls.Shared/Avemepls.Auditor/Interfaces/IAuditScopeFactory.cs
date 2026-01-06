namespace Avemepls.Auditor.Interfaces;

/// <summary>
/// Factory to create audit scopes ("sessions")
/// </summary>
public interface IAuditScopeFactory
{
    Task<AuditScope> Create(Action<AuditScopeOptions>? optionsBuilder, CancellationToken cancellationToken);
}