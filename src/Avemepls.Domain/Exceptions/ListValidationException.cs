using Avemepls.Domain.Validators;

using FluentValidation.Results;

namespace Avemepls.Domain.Exceptions;

/// <summary>
/// Исключение по элементу списка списка
/// </summary>
public class ListValidationException : Exception
{
    public ListItemValidationResult[] Errors { get; set; }

    public ListValidationException(ListItemValidationResult[] errors)
    {
        Errors = errors;
    }

    public ListValidationException(ListItemValidationResult error)
    {
        Errors = [error];
    }

    public ListValidationException(int itemId, List<ValidationFailure> errors)
    {
        Errors = [new ListItemValidationResult(errors, itemId)];
    }
}