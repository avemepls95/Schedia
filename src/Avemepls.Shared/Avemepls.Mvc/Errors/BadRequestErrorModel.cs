namespace Avemepls.Mvc.Errors;

/// <summary>
/// Модель 400 ошибки
/// </summary>
public class BadRequestErrorModel
{
    /// <summary>
    /// Ошибки заполнения полей
    /// </summary>
    public Dictionary<string, string[]>? ModelState { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? Message { get; set; }
}