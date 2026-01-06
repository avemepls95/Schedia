using Avemepls.Mvc.Filters;
using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

/// <summary>
/// Фильтр для исключения <see cref="ListValidationExceptionEndpointFilter"/>
/// </summary>
public class ListValidationExceptionEndpointFilter : ExceptionEndpointFilterBase<ListValidationException>
{
    protected override IResult Handle(ListValidationException exception)
    {
        return Results.Json(ListValidationExceptionFilter.GetError(exception), statusCode: StatusCodes.Status400BadRequest);
    }
}