using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class DeleteMinimalApiBuilder<TEntity, TId>(string routeName) : MinimalApiBuilderBase(routeName + "/{id}")
    where TEntity : IHasId<TId>, new()
{
    protected override string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(typeof(TEntity));
    }

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(typeof(TEntity));
    }

    protected override string HttpMethod => HttpMethods.Delete;

    protected override Delegate GetDelegate()
    {
        return Execute;
    }

    private static Task Execute([FromRoute] TId id, [FromServices] IMediator mediator)
    {
        var request = new DeleteCommand<TEntity, TId>(id);

        return mediator.Send(request);
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        var entityType = typeof(TEntity);

        routeHandlerBuilder
            .WithSummary($"Удалить '{entityType.GetXmlDocsSummary()}'")
            .WithName($"Delete{entityType.Name}")
            .WithOpenApi()
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}