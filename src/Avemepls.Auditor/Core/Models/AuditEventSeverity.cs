using System.ComponentModel.DataAnnotations;

namespace Avemepls.Auditor.Core.Models;

/// <summary>
/// Represents the severity level of an audit log entry.
/// </summary>
public enum AuditEventSeverity
{
    /// <summary>
    /// Represents cases where the severity level of the audit log entry cannot be determined or is not applicable.
    /// </summary>
    [Display(Name = "Обычная")]
    Unknown = 0,

    /// <summary>
    /// Represents the highest level of severity, indicating a critical issue that requires immediate attention.
    /// </summary>
    [Display(Name = "Критическая")]
    Critical,

    /// <summary>
    /// Represents a severe issue that requires prompt action.
    /// </summary>
    [Display(Name = "Высокая")]
    High,

    /// <summary>
    /// Represents an issue that requires attention but may not be critical or severe.
    /// </summary>
    [Display(Name = "Средняя")]
    Medium,

    /// <summary>
    /// Represents a minor issue that does not require immediate action but should be noted for future reference.
    /// </summary>
    [Display(Name = "Низкая")]
    Low,

    /// <summary>
    /// Represents non-critical, informative messages.
    /// </summary>
    [Display(Name = "Информация")]
    Info,

    /// <summary>
    /// Represents detailed debugging or tracing information for development or testing purposes.
    /// </summary>
    [Display(Name = "Отладка")]
    Debug
}