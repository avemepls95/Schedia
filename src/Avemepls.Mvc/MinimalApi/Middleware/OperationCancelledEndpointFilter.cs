using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

public class OperationCancelledEndpointFilter : ExceptionEndpointFilterBase<OperationCanceledException>
{
    protected override IResult Handle(OperationCanceledException exception)
    {
        return Results.StatusCode(499);
    }
}