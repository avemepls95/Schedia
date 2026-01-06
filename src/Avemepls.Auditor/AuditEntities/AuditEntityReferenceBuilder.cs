using System.Linq.Expressions;

using Avemepls.Auditor.AuditEntities.Common;
using Avemepls.Auditor.Builders;
using Avemepls.Auditor.DataAccess.Models;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auditor.AuditEntities;

public class AuditEntityReferenceBuilder<TEntity, TProperty>(
    Expression<Func<TEntity, TProperty>> propertySelector,
    IStringLocalizer loc)
    : AuditEntityPropertyBuilderBase<TEntity, TProperty>(propertySelector, loc)
{
    private readonly Expression<Func<TEntity, TProperty>> _propertySelector = propertySelector;

    private Delegate? _valueSelector;
    private bool _autoInclude = true;

    public AuditEntityReferenceBuilder<TEntity, TProperty> WithReadableValue<TValue>(
        Expression<Func<TProperty, TValue>> valueSelector)
    {
        _valueSelector = valueSelector.Compile();

        return this;
    }

    public AuditEntityReferenceBuilder<TEntity, TProperty> AutoInclude(bool autoInclude = true)
    {
        _autoInclude = autoInclude;

        return this;
    }

    public override async Task<AuditEvent?> TryBuildEvent(
        EntityEntry entityEntry,
        CancellationToken cancellationToken = default)
    {
        var propertyName = _propertySelector.Body.ToString().Split('.').Skip(1).Last();

        var nestedEntry = await LoadReferences(entityEntry, cancellationToken);

        if (nestedEntry is null)
        {
            return null;
        }

        if (nestedEntry.Reference(propertyName).Metadata is not INavigation navigation)
        {
            throw new ArgumentException($"{propertyName} of {typeof(TEntity).Name} is not reference");
        }

#pragma warning disable CA1826
        var foreignKey = navigation.ForeignKey.Properties.FirstOrDefault() ??
#pragma warning restore CA1826
                         throw new ArgumentException(
                             $"{propertyName} of {typeof(TEntity).Name} is not foreign key");

        var oldForeignKey = nestedEntry.OriginalValues[foreignKey.Name];
        var currentForeignKey = nestedEntry.CurrentValues[foreignKey.Name];

        string? oldReadableValue = null;
        string? currentReadableValue = null;

        if (oldForeignKey is null && currentForeignKey is null)
        {
            return null;
        }

        if (oldForeignKey?.Equals(currentForeignKey) != true)
        {
            if (_valueSelector is not null)
            {
                nestedEntry.CurrentValues[foreignKey.Name] = oldForeignKey;

                await nestedEntry.Reference(propertyName).LoadAsync(cancellationToken);

                var navigationEntity = nestedEntry.Reference(propertyName).CurrentValue;

                if (navigationEntity is not null)
                {
                    oldReadableValue = _valueSelector.DynamicInvoke(navigationEntity)?.ToString();
                }

                nestedEntry.CurrentValues[foreignKey.Name] = currentForeignKey;
                await nestedEntry.Reference(propertyName).LoadAsync(cancellationToken);

                navigationEntity = nestedEntry.Reference(propertyName).CurrentValue;

                if (navigationEntity is not null)
                {
                    currentReadableValue = _valueSelector.DynamicInvoke(navigationEntity)?.ToString();
                }
            }

            var oldPropertyValue = new AuditEventBuilder.PropertyValue(
                oldForeignKey,
                () => oldReadableValue!);

            var newPropertyValue = new AuditEventBuilder.PropertyValue(
                currentForeignKey,
                () => currentReadableValue!);

            var @event = BuildEvent(oldPropertyValue, newPropertyValue);

            return @event;
        }

        return null;
    }

    private async Task<EntityEntry?> LoadReferences(EntityEntry entityEntry, CancellationToken cancellationToken)
    {
        var propertyNames = _propertySelector.Body.ToString().Split('.').Skip(1).SkipLast(1);

        foreach (var propertyName in propertyNames)
        {
            if (entityEntry.Reference(propertyName).Metadata is not INavigation)
            {
                throw new ArgumentException($"{propertyName} of {typeof(TEntity).Name} is not reference");
            }

            if (_autoInclude)
            {
                await entityEntry.Reference(propertyName).LoadAsync(cancellationToken);
            }

            var targetEntry = entityEntry.Reference(propertyName).TargetEntry;

            if (targetEntry is null)
            {
                return null;
            }

            entityEntry = targetEntry;
        }

        return entityEntry;
    }
}