namespace Avemepls.Core.Models;

public interface ILimitQuery
{
    /// <summary>
    /// Offset from first record.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Number of records to take.
    /// </summary>
    public int? Limit { get; }
}