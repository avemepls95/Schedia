namespace Avemepls.Core.Models;

public interface IHasIncludeNonActive
{
    /// <summary>
    /// Показывать неактивные
    /// </summary>
    public bool IncludeNonActive { get; set; }
}