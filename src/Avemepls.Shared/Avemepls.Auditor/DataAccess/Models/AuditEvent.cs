using System.Net;

using Avemepls.Auditor.Core.Models;
using Avemepls.Core.DataAccess.Models;
using Avemepls.Core.Models;

namespace Avemepls.Auditor.DataAccess.Models;

/// <summary>
/// Audit event. Represents short user's session of interaction with system's API.
/// </summary>
public class AuditEvent : Entity<int>, IHasId
{
    /// <summary>
    /// Source of event (subsystem, process etc.)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Type of event (for example, "Login", "ReportBuilding", "PropertyChange", "StatusChange" etc.)
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Serverity of event. See <see cref="AuditEventSeverity"/> for details
    /// </summary>
    public AuditEventSeverity Severity { get; set; }

    /// <summary>
    /// Correlation ID from corresponding session. This could be real correlation ID from HTTP-request or other some "session" understanding
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Source IP address
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// ID/login for user
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// User's readable name
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Operation start date
    /// </summary>
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// Type of entity (domain entity) this event is relating to
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of domain entity
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Type of inner/child entity (may be child or related to domain entity)
    /// </summary>
    public string? InnerEntityType { get; set; }

    /// <summary>
    /// ID for inner/child entity
    /// </summary>
    public string? InnerEntityId { get; set; }

    /// <summary>
    /// Some message or comment for audit event
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Name of parameter that is changing
    /// </summary>
    public string? Parameter { get; set; }

    /// <summary>
    /// Old value of parameter (for example, foreign key or other)
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Old readable value of parameter (code of entity, readable date and time and other)
    /// </summary>
    public string? OldValueReadable { get; set; }

    /// <summary>
    /// New value of parameter
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// New readable value of parameter
    /// </summary>
    public string? NewValueReadable { get; set; }

    /// <summary>
    /// Custom payload data (json, html, source message etc)
    /// </summary>
    public string? Payload { get; set; }
}