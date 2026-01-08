using System.Security.Claims;

using Avemepls.Auditor.Core.Notifications;
using Avemepls.Core.Accumulator;
using Avemepls.Core.Models;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Security.Principal;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Constants = Avemepls.Security.Principal.Constants;

namespace Avemepls.Auditor.DataAccess.DbContextAuditor;

/// <summary>
///     <para>
///         Interceptor to automatically fill audit fields (created/modified/deleted date, created user).
///         Also allows to file Mediator events before and after entities update.
///     </para>
/// </summary>
public class DbContextAuditorInterceptor(
    IMediator mediator,
    HashSet<Type> auditableTypes,
    ICurrentDateTimeProvider currentDateTimeProvider,
    IPrincipalAccessor principalAccessor)
    : SaveChangesInterceptor
{
    public static string UserIdClaimKey { get; set; } = Constants.ClaimTypes.UserId;

    public static string UserIdentityIdClaimKey { get; set; } = ClaimTypes.NameIdentifier;

    public static string UserNameClaimKey { get; set; } = Constants.ClaimTypes.FullName;

    private AuditableEntry[]? _modifiedEntries;

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            ProcessSavingChanges(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await ProcessSavingChanges(eventData.Context, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null && result > 0)
        {
            ProcessSavedChanges(CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null && result > 0)
        {
            await ProcessSavedChanges(cancellationToken);
        }

        return result;
    }

    private async Task ProcessSavingChanges(DbContext context, CancellationToken cancellationToken)
    {
        _modifiedEntries = context.ChangeTracker
            .Entries()
            .Where(x => x.State != EntityState.Unchanged)
            .Select(x => new AuditableEntry(x.State, x))
            .ToArray();

        if (_modifiedEntries.Any())
        {
            await UpdateAuditFields(_modifiedEntries, currentDateTimeProvider, principalAccessor);
            await FireEntityEvents(_modifiedEntries, typeof(EntityUpdatingNotification<>), cancellationToken);
        }
    }

    private async Task ProcessSavedChanges(CancellationToken cancellationToken)
    {
        if (_modifiedEntries != null && _modifiedEntries.Length > 0)
        {
            await FireEntityEvents(_modifiedEntries, typeof(EntityUpdatedNotification<>), cancellationToken);
        }

        _modifiedEntries = [];
    }

    private sealed class User
    {
        public int? Id { get; set; }

        public string? IdentityId { get; set; }

        public string? UserName { get; set; }
    }

    private User? _user;

    private async ValueTask UpdateAuditFields(
        IEnumerable<AuditableEntry> entries,
        ICurrentDateTimeProvider currentDateTimeProvider,
        IPrincipalAccessor principalAccessor)
    {
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IHasDateCreated newEntry && newEntry.DateCreated == default)
                {
                    newEntry.DateCreated = currentDateTimeProvider.Now;
                }

                if (entry.Entity is IHasUserCreated<int> { UserCreatedId: 0 } hasUser)
                {
                    await TryInitUser(principalAccessor);

                    if (_user?.Id is not null)
                    {
                        hasUser.UserCreatedId = _user.Id.Value;
                    }
                }
                else if (entry.Entity is IHasUserCreated<int?> { UserCreatedId: null } hasUserNullable)
                {
                    await TryInitUser(principalAccessor);

                    hasUserNullable.UserCreatedId = _user?.Id;
                }
            }

            if (entry.State is EntityState.Modified or EntityState.Added &&
                entry.Entity is IHasDateModified modifiedEntry)
            {
                modifiedEntry.DateModified = currentDateTimeProvider.Now;
            }

            if (entry is { State: EntityState.Deleted, Entity: IHasDateDeleted deletedEntry })
            {
                deletedEntry.DateDeleted = currentDateTimeProvider.Now;
                entry.EntityEntry.State = EntityState.Modified;
            }
        }
    }

    private async Task TryInitUser(IPrincipalAccessor principalAccessor)
    {
        if (_user is null || _user.Id is null)
        {
            var user = await principalAccessor.GetPrincipal();

            var userId = user?.GetClaimValue(UserIdClaimKey);

            _user = new User
            {
                Id = userId is not null
                    ? int.Parse(userId)
                    : null,
                IdentityId = user?.GetClaimValue(UserIdentityIdClaimKey),
                UserName = user?.GetClaimValue(UserNameClaimKey)
            };
        }
    }

    /// <summary>
    /// Fires entity changed events to mediator pipeline
    /// </summary>
    /// <param name="modifiedEntries">List of modified entities</param>
    /// <param name="eventType">Type of event to fire (updating or updated)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task FireEntityEvents(
        IEnumerable<AuditableEntry> modifiedEntries,
        Type eventType,
        CancellationToken cancellationToken)
    {
        foreach (var entry in modifiedEntries.Where(e => auditableTypes.Contains(e.Entity.GetType())))
        {
            var genericType = eventType.MakeGenericType(entry.Entity.GetType());
            var evt = Activator.CreateInstance(genericType, entry.Entity, MapEfState(entry.State));

            if (evt != null)
            {
                await mediator.Publish(evt, cancellationToken);

                var accumulator = IScopedAggregatedEventsContainerContext.GetInstance();

                if (accumulator is not null)
                {
                    await accumulator.Accumulate(new AccumulateEventRequest(
                                                     "entity",
                                                     entry.Entity.GetType().Name!,
                                                     evt),
                                                 cancellationToken);
                }
            }
        }
    }

    private static EntityAction MapEfState(EntityState state)
    {
        return state switch
        {
            EntityState.Added => EntityAction.Added,
            EntityState.Deleted => EntityAction.Deleted,
            EntityState.Modified => EntityAction.Modified,
            _ => throw new NotSupportedException()
        };
    }
}