using Avemepls.Auditor.DataAccess.Models;

namespace Avemepls.Auditor.DataAccess;

/// <summary>
/// Repository for store/retrieve audit events
/// </summary>
public interface IAuditEventsRepository : IDisposable
{
    /// <summary>
    /// Save list of events into store
    /// </summary>
    ValueTask<int> Save(IEnumerable<AuditEvent> events, CancellationToken cancellationToken);
}