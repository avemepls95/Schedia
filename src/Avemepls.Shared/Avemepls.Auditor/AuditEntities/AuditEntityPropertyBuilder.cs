using System.Linq.Expressions;

using Avemepls.Auditor.AuditEntities.Common;
using Avemepls.Auditor.Builders;
using Avemepls.Auditor.DataAccess.Models;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auditor.AuditEntities;

public class AuditEntityPropertyBuilder<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, IStringLocalizer loc)
    : AuditEntityPropertyBuilderBase<TEntity, TProperty>(propertySelector, loc)
{
    private Func<TProperty?, string>? _readableValue;
    private Func<TProperty?, TProperty?, (string? OldReadableValue, string? NewReadableValue)>? _readableValues;

    private Func<TProperty?, TProperty?, Task<(string? OldReadableValue, string? NewReadableValue)>>?
        _readableValuesAsync;

    public AuditEntityPropertyBuilder<TEntity, TProperty> WithReadableValue(
        Func<TProperty?, string>? readableValue)
    {
        _readableValue = readableValue;

        return this;
    }

    public AuditEntityPropertyBuilder<TEntity, TProperty> WithReadableValue(
        Func<TProperty?, TProperty?, (string? OldReadableValue, string? NewReadableValue)>? readableValue)
    {
        _readableValues = readableValue;

        return this;
    }

    public AuditEntityPropertyBuilder<TEntity, TProperty> WithReadableValue(
        Func<TProperty?, TProperty?, Task<(string? OldReadableValue, string? NewReadableValue)>>? readableValue)
    {
        _readableValuesAsync = readableValue;

        return this;
    }

    public override async Task<AuditEvent?> TryBuildEvent(
        EntityEntry entityEntry,
        CancellationToken cancellationToken = default)
    {
        var oldValue = entityEntry.OriginalValues[PropertyInfo.Name];
        var currentValue = entityEntry.CurrentValues[PropertyInfo.Name];

        if (oldValue?.Equals(currentValue) != true)
        {
            var readableValues = await GetReadableValues(oldValue, currentValue);

            var oldPropertyValue =
                new AuditEventBuilder.PropertyValue(oldValue, () => readableValues.OldReadableValue!);

            var newPropertyValue =
                new AuditEventBuilder.PropertyValue(currentValue, () => readableValues.NewReadableValue!);

            var @event = BuildEvent(oldPropertyValue, newPropertyValue);

            return @event;
        }

        return null;
    }

    private async Task<(string? OldReadableValue, string? NewReadableValue)> GetReadableValues(
        object? oldValue,
        object? currentValue)
    {
        (string? OldReadableValue, string? NewReadableValue) readableValues = (null, null);

        if (oldValue is TProperty old && currentValue is TProperty @new)
        {
            if (_readableValues is not null)
            {
                readableValues = _readableValues.Invoke(old, @new);
            }
            else if (_readableValue is not null)
            {
                readableValues = (_readableValue.Invoke(old), _readableValue.Invoke(@new));
            }
            else if (_readableValuesAsync is not null)
            {
                readableValues = await _readableValuesAsync.Invoke(old, @new);
            }
        }

        return readableValues;
    }
}