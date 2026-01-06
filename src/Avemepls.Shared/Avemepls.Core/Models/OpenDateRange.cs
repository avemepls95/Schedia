using System.Diagnostics;

namespace Avemepls.Core.Models;

/// <summary>
/// Открытый интервал дат (со временем)
/// </summary>
[DebuggerDisplay("{From}-{To}")]
public struct OpenDateRange
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="OpenDateRange"/>.
    /// </summary>
    public OpenDateRange(DateTime? @from, DateTime? to)
    {
        From = @from;
        To = to;
    }

    /// <summary>
    /// Начало отрезка включительно.
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// Конец отрезка включительно.
    /// </summary>
    public DateTime? To { get; init; }
}