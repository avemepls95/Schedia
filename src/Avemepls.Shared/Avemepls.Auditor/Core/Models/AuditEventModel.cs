using Avemepls.Core.Models;

namespace Avemepls.Auditor.Core.Models;

public class AuditEventModel : IHasId<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Source of event (subsystem, process etc.)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Type of event (for example, "Login", "ReportBuilding", "PropertyChange", "StatusChange" etc)
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Severity of event
    /// </summary>
    public AuditEventSeverity Severity { get; set; }

    /// <summary>
    /// Correlation ID from corresponding session. This could be real correlation ID from HTTP-request or other some "session" understanding
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Source IP address
    /// </summary>
    public string? IpAddress { get; set; }

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
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Type of high-level entity (domain entity) this event is relates to
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of high-level domain entity
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Type of low-level entity (may be child or related to domain entity)
    /// </summary>
    public string? InnerEntityType { get; set; }

    /// <summary>
    /// ID for low-level entity
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