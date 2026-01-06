using FluentValidation;
using FluentValidation.Results;

namespace Avemepls.Domain.Validators;

public static class ValidationContextExtensions
{
    public static void AddFailureWithCode<T>(
        this ValidationContext<T> validationContext,
        string errorCode,
        string errorMessage)
    {
        var failure = new ValidationFailure(validationContext.PropertyPath, errorMessage)
        {
            ErrorCode = errorCode
        };

        validationContext.AddFailure(failure);
    }
}