using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

using Avemepls.Auditor.AuditEntities.Common;
using Avemepls.Auditor.DataAccess.Models;
using Avemepls.Core.Models;

using Microsoft.Extensions.Localization;

using EntityEntry = Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry;

namespace Avemepls.Auditor.AuditEntities;

public abstract class AuditEntityProfile<TEntity>(IStringLocalizerFactory stringLocalizerFactory) : IAuditEntityProfile
    where TEntity : class, new()
{
    private readonly List<IAuditEntityBuilder> _builders = [];

    public AuditEntityPropertyBuilder<TEntity, TProperty> Track<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector)
    {
        var loc = stringLocalizerFactory.Create(typeof(AuditEntityPropertyBuilder<TEntity, TProperty>));
        var builder = new AuditEntityPropertyBuilder<TEntity, TProperty>(propertySelector, loc);

        _builders.Add(builder);

        return builder;
    }

    public AuditEntityPropertyBuilder<TEntity, TProperty> TrackEnum<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector)
    {
        return Track(propertySelector)
            .WithReadableValue(x => x?.GetType().GetCustomAttribute<DisplayAttribute>()?.Name ?? x?.ToString()!);
    }

    public AuditEntityReferenceBuilder<TEntity, TProperty> TrackReference<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector)
        where TProperty : class
    {
        var loc = stringLocalizerFactory.Create(typeof(AuditEntityReferenceBuilder<TEntity, TProperty>));
        var builder = new AuditEntityReferenceBuilder<TEntity, TProperty>(propertySelector, loc);

        _builders.Add(builder);

        return builder;
    }

    public AuditEntityReferenceBuilder<TEntity, TProperty> TrackDictionaryCode<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector)
        where TProperty : class, IHasCode
    {
        return TrackReference(propertySelector).WithReadableValue(x => x.Code);
    }

    public async Task<AuditEvent[]> BuildEvents(EntityEntry entityEntry, CancellationToken cancellationToken = default)
    {
        var events = new List<AuditEvent>();

        foreach (var builder in _builders)
        {
            var @event = await builder.TryBuildEvent(entityEntry, cancellationToken);

            if (@event is not null)
            {
                events.Add(@event);
            }
        }

        return events.ToArray();
    }

    public Type EntityType { get; } = typeof(TEntity);
}