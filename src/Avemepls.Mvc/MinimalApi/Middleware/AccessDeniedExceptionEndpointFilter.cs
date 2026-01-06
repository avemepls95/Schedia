using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

/// <summary>
/// Фильтр для исключения <see cref="AccessDeniedException"/>.
/// </summary>
public class AccessDeniedExceptionEndpointFilter : ExceptionEndpointFilterBase<AccessDeniedException>
{
    protected override IResult Handle(AccessDeniedException exception)
    {
        return Results.Forbid();
    }
}