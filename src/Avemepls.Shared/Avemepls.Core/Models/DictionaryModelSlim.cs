using Avemepls.Core.Localization;

namespace Avemepls.Core.Models;

/// <summary>
/// Базовая модель словарной сущности для запросов в списках
/// </summary>
public abstract class DictionaryModelSlim : IHasCode, IHasName, IHasId
{
    public int Id { get; set; }

    /// <summary>
    /// Полное название
    /// </summary>
    [DisplayNameLoc("Полное название")]
    public string? FullName { get; set; }

    /// <summary>
    /// Цветовая метка
    /// </summary>
    [DisplayNameLoc("Цвет")]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Сортировка
    /// </summary>
    [DisplayNameLoc("Сортировка")]
    public decimal SortOrder { get; set; }

    /// <summary>
    /// Код
    /// </summary>
    [DisplayNameLoc("Код")]
    public string Code { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [DisplayNameLoc("Название")]
    public string Name { get; set; }
}