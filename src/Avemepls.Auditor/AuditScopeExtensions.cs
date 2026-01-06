using Avemepls.Auditor.Builders;

namespace Avemepls.Auditor;

public static class AuditScopeExtensions
{
    /// <summary>
    /// Create audit event for entity owning. If event type is specified, event will be immediately added to audit log.
    /// </summary>
    public static AuditScope AddEntityEvent<TEntity, TId>(
        this AuditScope scope,
        TId? entityId,
        string? eventType,
        Action<AuditEventBuilder>? builderCfg = null)
    {
        var auditEvent = scope.CreateEvent();
        var builder = new AuditScopeEventBuilder(scope, auditEvent);

        if (entityId is not null)
            builder.WithEntity<TEntity, TId>(entityId);
        else
            builder.WithEntity<TEntity>();

        if (eventType is not null)
            builder.EventType(eventType);

        builderCfg?.Invoke(builder);

        builder.Finish();

        return scope;
    }

    public static AuditScope AddEntityEvent<TEntity, TId>(
        this AuditScope scope,
        TId? entityId,
        Action<AuditEventBuilder>? builderCfg = null)
    {
        return scope.AddEntityEvent<TEntity, TId>(entityId, null, builderCfg);
    }

    public static AuditScope AddEntityEvent<TEntity>(
        this AuditScope scope,
        string eventType,
        Action<AuditEventBuilder>? builderCfg = null)
    {
        return scope.AddEntityEvent<TEntity, int?>(null, eventType, builderCfg);
    }

    /// <summary>
    /// Create custom audit event
    /// </summary>
    public static AuditScope AddEvent(this AuditScope scope, string eventType, Action<AuditEventBuilder>? builderCfg = null)
    {
        var auditEvent = scope.CreateEvent();
        var builder = new AuditScopeEventBuilder(scope, auditEvent);
        builder.EventType(eventType);
        builderCfg?.Invoke(builder);
        builder.Finish();

        return scope;
    }
}