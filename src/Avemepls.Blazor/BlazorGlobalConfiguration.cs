namespace Avemepls.Blazor;

/// <summary>
/// Глобальная конфигурация компонентов
/// </summary>
public static class BlazorGlobalConfiguration
{
    /// <summary>
    /// Задержка для аггрегации введеных данных перед отправкой на сервер в мс.
    /// </summary>
    public static int InputDebounceMilliseconds { get; set; } = 750;

    /// <summary>
    /// Форматы ввода дат для DatePicker
    /// </summary>
    public static string DatePickerFormat { get; set; } = "dd.MM.yyyy";

    /// <summary>
    /// Стиль деактивированной строки
    /// </summary>
    public static string DeactivatedStyle { get; set; } = "row-deactivated ";

    /// <summary>
    /// Стиль удаленной строки
    /// </summary>
    public static string DeletedStyle { get; set; } = "row-deleted ";
}