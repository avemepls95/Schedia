using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class UpdateCommandMinimalApiBuilder<TRequest, TId>(string routeName)
    : BaseMediatrMinimalApiBuilder<TRequest, TId>(routeName + "/{id}")
    where TRequest : IUpdateCommand<TId>, IRequest<TId>, new()
    where TId : struct
{
    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Put;

    protected override Delegate GetDelegate() => ExecuteUpdate;

    private static Task<TId> ExecuteUpdate(
        [FromRoute] TId id,
        [FromBody] TRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        request.Id = id;

        return mediator.Send(request, cancellationToken);
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .Produces<TId>()
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DescriptionFrom(RequestType);
    }
}