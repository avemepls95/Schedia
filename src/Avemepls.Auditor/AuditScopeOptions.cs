using System.Net;

namespace Avemepls.Auditor;

/// <summary>
/// Options for audit scope. Used to construct base data in audit events.
/// </summary>
public class AuditScopeOptions
{
    internal string? UserId { get; set; }

    internal string? UserName { get; set; }

    internal IPAddress? IpAddress { get; set; }

    internal string? CorrelationId { get; set; }

    /// <summary>
    /// Specify user id for audit event
    /// </summary>
    public AuditScopeOptions WithUserId(string userId)
    {
        UserId = userId;
        return this;
    }

    /// <summary>
    /// Specify user name for audit event
    /// </summary>
    public AuditScopeOptions WithUserName(string userName)
    {
        UserName = userName;
        return this;
    }

    /// <summary>
    /// Specify IP-address for audit event
    /// </summary>
    public AuditScopeOptions WithIpAddress(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
        return this;
    }

    /// <summary>
    /// Specify correlation id for audit event
    /// </summary>
    public AuditScopeOptions WithCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }
}