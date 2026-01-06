using Avemepls.Mvc.Errors;
using Avemepls.Mvc.Filters.Base;

using FluentValidation.Results;

using ValidationException = FluentValidation.ValidationException;

namespace Avemepls.Mvc.Filters;

/// <summary>
/// Отлавливает ошибки валидации.
/// </summary>
public class ValidationExceptionFilter : BadRequestExceptionFilterBase<ValidationException>
{
    internal override BadRequestErrorModel GetError(ValidationException exception)
    {
        var error = new BadRequestErrorModel();

        if (!exception.Errors.Any())
        {
            error.Message = exception.Message;
        }
        else
        {
            foreach (var validationFailure in exception.Errors)
            {
                ChangeUpperCase(validationFailure);
            }

            error.ModelState = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(e => e.ErrorMessage).ToArray());
        }

        return error;
    }

    public static void ChangeUpperCase(ValidationFailure validationFailure)
    {
        if (!string.IsNullOrEmpty(validationFailure.PropertyName))
        {
            var properties = validationFailure.PropertyName.Split(".");

            var joinedLowerCasedProperties = string.Join(".",
                                                         properties.Select(a => string.Concat(a[0].ToString().ToLower(), a.AsSpan(1))));

            validationFailure.PropertyName = joinedLowerCasedProperties;
        }
    }
}