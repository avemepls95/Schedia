using Avemepls.Mvc.Errors;
using Avemepls.Mvc.Filters;
using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

/// <summary>
/// Фильтр для исключения <see cref="ApplicationException"/>
/// </summary>
public class ApplicationExceptionEndpointFilter : BadRequestExceptionEndpointFilterBase<ApplicationException>
{
    private static readonly ApplicationExceptionFilter ApplicationExceptionFilter = new();

    protected override BadRequestErrorModel GetError(ApplicationException exception)
    {
        return ApplicationExceptionFilter.GetError(exception);
    }
}