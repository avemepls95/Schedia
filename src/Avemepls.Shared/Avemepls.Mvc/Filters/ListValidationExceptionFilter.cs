using Avemepls.Domain.Exceptions;
using Avemepls.Mvc.Errors;
using Avemepls.Mvc.Filters.Base;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters;

public class ListValidationExceptionFilter : ExceptionFilter<ListValidationException>
{
    protected override void Handle(ListValidationException exception, ExceptionContext context)
    {
        var errorModel = GetError(exception);
        context.Result = new BadRequestObjectResult(errorModel);
    }

    public static IEnumerable<ListBadRequestErrorModel> GetError(ListValidationException exception)
    {
        foreach (var error in exception.Errors)
        {
            foreach (var validationFailure in error.Errors)
            {
                ValidationExceptionFilter.ChangeUpperCase(validationFailure);
            }

            yield return new ListBadRequestErrorModel()
            {
                ItemId = error.ItemId,
                Errors = new BadRequestErrorModel()
                {
                    ModelState = error.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Select(e => e.ErrorMessage).ToArray())
                }
            };
        }
    }
}

public class ListBadRequestErrorModel
{
    /// <summary>
    /// Идентификатор строки
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// Ошибки заполнения полей
    /// </summary>
    public BadRequestErrorModel Errors { get; set; }
}