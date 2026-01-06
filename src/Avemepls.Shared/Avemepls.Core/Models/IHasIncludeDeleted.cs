namespace Avemepls.Core.Models;

public interface IHasIncludeDeleted
{
    /// <summary>
    /// Показывать удаленные
    /// </summary>
    public bool IncludeDeleted { get; set; }
}