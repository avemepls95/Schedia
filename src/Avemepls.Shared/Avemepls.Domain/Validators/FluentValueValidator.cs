using FluentValidation;

namespace Avemepls.Domain.Validators;

/// <summary>
/// A glue class to make it easy to define validation rules for single values using FluentValidation
/// </summary>
public class FluentValueValidator<T> : AbstractValidator<T>
{
    public FluentValueValidator(Action<IRuleBuilderInitial<T, T>> rule)
    {
        rule(RuleFor(x => x));
    }

    private IEnumerable<string> ValidateValue(T arg)
    {
        var result = base.Validate(arg);

        return result.IsValid
            ? []
            : result.Errors.Select(e => e.ErrorMessage);
    }

    public new Func<T, IEnumerable<string>> Validate => ValidateValue;
}