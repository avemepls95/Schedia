namespace Avemepls.Blazor.Navigation;

/// <summary>
/// Запись о навигации по приложению
/// </summary>
public record NavigationHistoryRecord(string PageName, string Url)
{
    /// <summary>
    /// Название страницы по маршруту
    /// </summary>
    public string PageName { get; } = PageName;

    /// <summary>
    /// Адрес страницы
    /// </summary>
    public string Url { get; } = Url;
}