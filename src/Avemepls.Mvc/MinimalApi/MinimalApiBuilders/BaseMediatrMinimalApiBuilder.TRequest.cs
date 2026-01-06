using System.Net.Mime;

using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.Middleware;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public abstract class BaseMediatrMinimalApiBuilder<TRequest>(string route) : MinimalApiBuilderBase(route)
    where TRequest : IRequest
{
    protected static Type RequestType { get; } = typeof(TRequest);

    protected override string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(RequestType);
    }

    protected override Delegate GetDelegate() => Execute;

    protected override string HttpMethod => HttpMethods.Post;

    private Func<TRequest, CancellationToken, Task<TRequest>>? _requestPreProcessor;

    public BaseMediatrMinimalApiBuilder<TRequest> WithRequestPreProcessor(
        Func<TRequest, CancellationToken, Task<TRequest>> func)
    {
        _requestPreProcessor = func;

        return this;
    }

    protected virtual async Task Execute(
        [FromBody]
        TRequest request,
        [FromServices]
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await PreProcessRequest(request, cancellationToken);

        await mediator.Send(request, cancellationToken);
    }

    protected virtual Task PreProcessRequest(TRequest request, CancellationToken cancellationToken)
    {
        return _requestPreProcessor?.Invoke(request, cancellationToken) ?? Task.CompletedTask;
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .Accepts(RequestType, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter(MinimalApiFilters.ValidationEndpointFilter)
            .DescriptionFrom(RequestType);
    }
}