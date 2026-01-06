using Avemepls.Auditor.DataAccess;
using Avemepls.Auditor.DataAccess.Models;
using Avemepls.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Auditor;

/// <summary>
/// Scope for auditing events. Represents kind of "session" with chunk of events occured during this session.
/// </summary>
public class AuditScope : IAsyncDisposable
{
    private readonly AuditScopeOptions _options;
    private readonly List<AuditEvent> _events = [];
    private readonly IServiceScope _serviceScope;
    private readonly ICurrentDateTimeProvider _dateTimeProvider;
    private readonly IAuditEventsRepository _repository;

    public AuditScope(
        AuditScopeOptions options,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceScope = serviceProvider.CreateScope();
        _dateTimeProvider = _serviceScope.ServiceProvider.GetRequiredService<ICurrentDateTimeProvider>();
        _repository = _serviceScope.ServiceProvider.GetRequiredService<IAuditEventsRepository>();
    }

    internal AuditEvent CreateEvent()
    {
        var auditEvent = new AuditEvent
        {
            CorrelationId = _options.CorrelationId,
            UserId = _options.UserId,
            IpAddress = _options.IpAddress,
            UserName = _options.UserName
        };

        return auditEvent;
    }

    internal AuditScope AddEvent(AuditEvent auditEvent)
    {
        auditEvent.DateTime = _dateTimeProvider.Now;
        _events.Add(auditEvent);

        return this;
    }

    public async ValueTask DisposeAsync()
    {
        // Perform async cleanup.
        await DisposeAsyncCore();

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _repository.Save(_events, CancellationToken.None);
        _repository.Dispose();
        _serviceScope.Dispose();
    }
}