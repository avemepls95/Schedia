namespace Avemepls.Domain.Models;

/// <summary>
/// Модель для сортировки
/// </summary>
public class PropertySortInfo
{
    /// <summary>
    /// Название свойства сущности (модели)
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// True - от большего к меньшему, False - от меньшему к большему
    /// </summary>
    public bool Descending { get; set; }
}