using Avemepls.Auditor.DataAccess.Models;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Avemepls.Auditor.AuditEntities.Common;

public interface IAuditEntityBuilder
{
    Task<AuditEvent?> TryBuildEvent(EntityEntry entityEntry, CancellationToken cancellationToken = default);
}