using Avemepls.Auditor.AuditEntities.Common;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auditor.AuditEntities;

internal sealed class AuditEntityPropertyManager
{
    private readonly Dictionary<Type, IAuditEntityProfile> _profiles;

    public AuditEntityPropertyManager(IEnumerable<IAuditEntityProfile> profiles)
    {
        _profiles = profiles.ToDictionary(x => x.EntityType);
    }

    public async Task Build(AuditScope scope, DbContext dbContext, CancellationToken cancellationToken = default)
    {
        dbContext.ChangeTracker.DetectChanges();

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (_profiles.TryGetValue(entry.Entity.GetType(), out var profile))
            {
                var events = await profile.BuildEvents(entry, cancellationToken);

                foreach (var @event in events)
                {
                    scope.AddEvent(@event);
                }
            }
        }
    }
}