using FluentValidation;
using FluentValidation.Results;

namespace Avemepls.Domain.Exceptions;

/// <summary>
/// Исключение генерируемое при нарушении бизнес-логики.
/// </summary>
public class BusinessRuleException : ValidationException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusinessRuleException"/>.
    /// </summary>
    public BusinessRuleException(string message) : base(message)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusinessRuleException"/>.
    /// </summary>
    public BusinessRuleException(ValidationFailure validationFailure)
        : base(new List<ValidationFailure>([validationFailure]))
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusinessRuleException"/>.
    /// </summary>
    public BusinessRuleException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusinessRuleException"/>.
    /// </summary>
    public BusinessRuleException(IEnumerable<ValidationFailure> errors)
        : base(errors)
    {
    }
}