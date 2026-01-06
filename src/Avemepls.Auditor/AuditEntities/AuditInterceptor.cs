using Avemepls.Auditor.Interfaces;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Avemepls.Auditor.AuditEntities;

internal sealed class AuditInterceptor(IAuditScopeFactory auditScopeFactory, AuditEntityPropertyManager auditEntityPropertyManager)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        await using var auditScope = await auditScopeFactory.Create(optionsBuilder: null, cancellationToken);

        await auditEntityPropertyManager.Build(auditScope, eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}