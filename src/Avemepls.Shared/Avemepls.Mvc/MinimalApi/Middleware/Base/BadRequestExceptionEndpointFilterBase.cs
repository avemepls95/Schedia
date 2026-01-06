using Avemepls.Mvc.Errors;

namespace Avemepls.Mvc.MinimalApi.Middleware.Base;

public abstract class BadRequestExceptionEndpointFilterBase<TException> : ExceptionEndpointFilterBase<TException>
    where TException : Exception
{
    protected override IResult Handle(TException exception)
    {
        return Results.Json(GetError(exception), statusCode: StatusCodes.Status400BadRequest);
    }

    protected abstract BadRequestErrorModel GetError(TException exception);
}