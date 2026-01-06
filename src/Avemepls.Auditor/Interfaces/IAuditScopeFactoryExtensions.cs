namespace Avemepls.Auditor.Interfaces;

/// <summary>
/// Factory to create audit scopes ("sessions")
/// </summary>
public static class IAuditScopeFactoryExtensions
{
    public static Task<AuditScope> Create(this IAuditScopeFactory scopeFactory, CancellationToken cancellationToken)
    {
        return scopeFactory.Create(null, cancellationToken);
    }
}