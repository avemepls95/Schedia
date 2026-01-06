using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.Middleware;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class CreateCommandMinimalApiBuilder<TRequest, TId>(string route) : MinimalApiBuilderBase(route)
    where TRequest : ICreateCommand, IRequest<TId>, new()
    where TId : struct
{
    protected static Type RequestType { get; } = typeof(TRequest);

    protected override string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(RequestType);
    }

    private (Type GetByIdQyery, Type GetByIdDetailedModel)? _createdAtRoute;

    public void WithCreatedAt<TQuery, TDetailedModel>()
        where TQuery : IGetByIdQuery<TId>
    {
        _createdAtRoute = (typeof(TQuery), typeof(TDetailedModel));
    }

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Post;

    protected override Delegate GetDelegate() => Execute;

    private async Task<IResult> Execute([FromBody] TRequest request, [FromServices] IMediator mediator)
    {
        var id = await mediator.Send(request);

        if (_createdAtRoute is not null)
        {
            var query = Activator.CreateInstance(_createdAtRoute.Value.GetByIdQyery, id);
            var detailedModel = await mediator.Send(query!);

            return Results.Created($"{GetRoute()}/{id}", detailedModel);
        }

        return Results.Ok(id);
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .Produces<TId>()
            .DescriptionFrom(RequestType)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter(MinimalApiFilters.ValidationEndpointFilter);

        if (_createdAtRoute is not null)
        {
            routeHandlerBuilder.Produces(StatusCodes.Status200OK, _createdAtRoute.Value.GetByIdDetailedModel);
        }
        else
        {
            routeHandlerBuilder.Produces(StatusCodes.Status200OK, typeof(TId));
        }
    }
}