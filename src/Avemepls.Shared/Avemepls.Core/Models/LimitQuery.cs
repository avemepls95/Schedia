namespace Avemepls.Core.Models;

/// <summary>
/// Limit/offset query
/// </summary>
public class LimitQuery : ILimitQuery
{
    /// <summary>
    /// Offset from first record.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Number of records to take.
    /// </summary>
    public int? Limit { get; set; } = 20;
}