using FluentValidation;

namespace Avemepls.Domain.Validators;

public abstract class ExtendedAbstractValidator<T> : AbstractValidator<T>
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var context = ValidationContext<T>.CreateWithOptions((T)model, x => x.IncludeProperties(propertyName));
        var result = await ValidateAsync(context);

        return result.IsValid
            ? []
            : result.Errors.Select(e => e.ErrorMessage);
    };
}