using System.Diagnostics;

namespace Avemepls.Domain.Models;

/// <summary>
/// Открытый интервал дат (без времени)
/// </summary>
[DebuggerDisplay("{From}-{To}")]
public class OpenDateOnlyRange
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="OpenDateOnlyRange"/>.
    /// </summary>
    public OpenDateOnlyRange()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="OpenDateOnlyRange"/>.
    /// </summary>
    public OpenDateOnlyRange(DateOnly? @from, DateOnly? to)
    {
        From = @from;
        To = to;
    }

    /// <summary>
    /// Начало отрезка включительно.
    /// </summary>
    public DateOnly? From { get; set; }

    /// <summary>
    /// Конец отрезка включительно.
    /// </summary>
    public DateOnly? To { get; set; }
}