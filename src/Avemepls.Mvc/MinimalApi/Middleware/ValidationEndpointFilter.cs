using Avemepls.Mvc.Errors;
using Avemepls.Mvc.Filters;
using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

public class ValidationEndpointFilter : BadRequestExceptionEndpointFilterBase<ValidationException>
{
    private static readonly ValidationExceptionFilter ValidationExceptionFilter = new();

    protected override BadRequestErrorModel GetError(ValidationException exception)
    {
        return ValidationExceptionFilter.GetError(exception);
    }
}