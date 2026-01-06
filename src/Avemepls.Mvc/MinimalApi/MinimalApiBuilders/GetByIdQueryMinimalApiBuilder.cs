using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class GetByIdQueryMinimalApiBuilder<TRequest, TModel, TId>(string routeName)
    : MinimalApiBuilderBase(routeName + "/{id}")
    where TRequest : GetEntityByIdQuery<TModel, TId>
{
    protected static Type RequestType { get; } = typeof(TRequest);

    protected override string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(RequestType);
    }

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override Delegate GetDelegate() => Execute;

    private static Task<TModel> Execute([FromRoute] TId id, [FromServices] IMediator mediator)
    {
        return mediator.Send((TRequest)Activator.CreateInstance(RequestType, id)!);
    }

    protected override string HttpMethod => HttpMethods.Get;

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .DescriptionFrom(RequestType)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}