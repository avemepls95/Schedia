using Avemepls.Auditor.DataAccess.Models;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Avemepls.Auditor.AuditEntities.Common;

public interface IAuditEntityProfile
{
    Task<AuditEvent[]> BuildEvents(EntityEntry entityEntry, CancellationToken cancellationToken = default);

    Type EntityType { get; }
}