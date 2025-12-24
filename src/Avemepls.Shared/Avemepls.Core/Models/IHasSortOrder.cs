namespace Avemepls.Core.Models;

public interface IHasSortOrder
{
    /// <summary>
    /// Default sorting order on entity querying
    /// </summary>
    decimal SortOrder { get; }
}