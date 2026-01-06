using Avemepls.Domain.Queries;

namespace Avemepls.Auditor.Core.Models;

/// <summary>
/// Filter for searching audit events in store
/// </summary>
public class SearchAuditEventsQuery : SearchQuery<AuditEventModel>
{
    /// <summary>
    /// Source of event (subsystem, process etc.)
    /// </summary>
    public string[]? Sources { get; set; }

    /// <summary>
    /// Types of events
    /// </summary>
    public string[]? EventTypes { get; set; }

    /// <summary>
    /// Severities of events
    /// </summary>
    public AuditEventSeverity[]? Severities { get; set; }

    /// <summary>
    /// "Parent" or owner entity type
    /// </summary>
    public string[]? EntityTypes { get; set; }

    /// <summary>
    /// ID for parent entity
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Type of entity this event is directly related to
    /// </summary>
    public string? InnerEntityType { get; set; }

    /// <summary>
    /// ID of entity this event is directly related to
    /// </summary>
    public string? InnerEntityId { get; set; }

    /// <summary>
    /// Start date of event (inclusive)
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// End date of event (exclusive)
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Id of user who initiated this event
    /// </summary>
    public string? UserId { get; set; }
}