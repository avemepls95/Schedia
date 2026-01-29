using FluentValidation;

namespace Avemepls.Domain.Extensions;

public static class ValidationExceptionExtensions
{
    public static string GetFirstError(this ValidationException exception)
        => exception.Errors.FirstOrDefault()?.ErrorMessage ?? exception.Message;
}