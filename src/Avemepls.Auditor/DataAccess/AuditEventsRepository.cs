using Avemepls.Auditor.DataAccess.Models;

namespace Avemepls.Auditor.DataAccess;

public sealed class AuditEventsRepository(AuditDataContext context) : IAuditEventsRepository
{
    public async ValueTask<int> Save(IEnumerable<AuditEvent> events, CancellationToken cancellationToken)
    {
        await context.AuditEvents.AddRangeAsync(events, cancellationToken);

        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}