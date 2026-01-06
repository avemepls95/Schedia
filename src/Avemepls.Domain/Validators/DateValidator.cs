using FluentValidation;
using FluentValidation.Validators;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Валидатор дат.
/// </summary>
public class DateValidator<T> : PropertyValidator<T, DateTime>
{
    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public DateValidator()
    {
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return "'{PropertyName}' должно содержать только дату. Было указано '{AttemptedValue}' время.";
    }

    public override string Name { get; }

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, DateTime dateTime)
    {
        var isValid = dateTime.TimeOfDay == TimeSpan.Zero;

        if (!isValid)
        {
            context.MessageFormatter.AppendArgument("AttemptedValue", dateTime.TimeOfDay);
        }

        return isValid;
    }
}