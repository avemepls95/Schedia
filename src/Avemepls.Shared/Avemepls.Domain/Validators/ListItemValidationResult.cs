using FluentValidation.Results;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Результат валидации по строке из списка
/// </summary>
public class ListItemValidationResult(IEnumerable<ValidationFailure> errors, int itemId) : ValidationResult(errors)
{
    /// <summary>
    /// Идентификатор строки
    /// </summary>
    public int ItemId { get; set; } = itemId;
}