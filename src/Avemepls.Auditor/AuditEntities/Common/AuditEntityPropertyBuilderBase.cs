using System.Linq.Expressions;
using System.Reflection;

using Avemepls.Auditor.Builders;
using Avemepls.Auditor.Core.Models;
using Avemepls.Auditor.DataAccess.Models;
using Avemepls.Core.Extensions;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auditor.AuditEntities.Common;

public abstract class AuditEntityPropertyBuilderBase<TEntity, TProperty>(
    Expression<Func<TEntity, TProperty>> propertySelector,
    IStringLocalizer loc)
    : IAuditEntityBuilder
{
    public abstract Task<AuditEvent?> TryBuildEvent(EntityEntry entityEntry, CancellationToken cancellationToken = default);
    private AuditEventSeverity _severity = AuditEventSeverity.Info;

    protected PropertyInfo PropertyInfo { get; } = propertySelector.GetPropertyInfo();

    private string? _propertyName;
    private Action<AuditEventBuilder>? _eventBuilder;

    public AuditEntityPropertyBuilderBase<TEntity, TProperty> WithPropertyName(string propertyName)
    {
        _propertyName = propertyName;

        return this;
    }

    public AuditEntityPropertyBuilderBase<TEntity, TProperty> Configure(Action<AuditEventBuilder> configure)
    {
        _eventBuilder = configure;

        return this;
    }

    public AuditEntityPropertyBuilderBase<TEntity, TProperty> WithSeverity(AuditEventSeverity severity)
    {
        _severity = severity;

        return this;
    }

    protected AuditEvent BuildEvent(
        AuditEventBuilder.PropertyValue oldValue,
        AuditEventBuilder.PropertyValue newValue)
    {
        var propertyName = _propertyName ?? PropertyInfo.GetDisplayName();

        var eventBuilder = new AuditEventBuilder();

        eventBuilder.PropertyChanged(propertyName,
                                     oldValue,
                                     newValue);

        var message = string.Format(loc["Изменено {0}: {1} -> {2}"],
                                    propertyName,
                                    oldValue.ReadableValue ?? oldValue.Value,
                                    newValue.ReadableValue ?? newValue.Value);

        eventBuilder.Message(message);
        eventBuilder.WithSeverity(_severity);
        eventBuilder.WithEntity<TEntity>();

        _eventBuilder?.Invoke(eventBuilder);

        return eventBuilder.Build();
    }
}