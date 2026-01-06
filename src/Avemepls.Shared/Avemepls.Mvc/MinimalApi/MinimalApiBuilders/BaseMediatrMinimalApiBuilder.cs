using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public abstract class BaseMediatrMinimalApiBuilder<TRequest, TResponse>(string route) : MinimalApiBuilderBase(route)
    where TRequest : IRequest<TResponse>, new()
{
    private string? _groupName;

    protected static Type RequestType { get; } = typeof(TRequest);

    public BaseMediatrMinimalApiBuilder<TRequest, TResponse> WithGroupName(string groupName)
    {
        _groupName = groupName;

        return this;
    }

    protected override string GetGroupName()
    {
        return _groupName ?? MinimalApiHelper.GetGroupName(RequestType);
    }

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override Delegate GetDelegate() => HttpMethod == HttpMethods.Get
        ? ExecuteFromQuery
        : ExecuteFromBody;

    protected override string HttpMethod => HttpMethods.Post;

    private Func<TRequest, CancellationToken, Task<TRequest>>? _requestPreProcessor;

    public BaseMediatrMinimalApiBuilder<TRequest, TResponse> WithRequestPreProcessor(
        Func<TRequest, CancellationToken, Task<TRequest>> func)
    {
        _requestPreProcessor = func;

        return this;
    }

    protected virtual async Task<TResponse> ExecuteFromBody(
        [FromBody] TRequest? request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await Execute(request, mediator, cancellationToken);
    }

    protected virtual async Task<TResponse> ExecuteFromQuery(
        [AsParameters] // Can't use FromQuery because need implementation TRequest.TryParse
        TRequest? request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await Execute(request, mediator, cancellationToken);
    }

    private async Task<TResponse> Execute(TRequest? request, IMediator mediator, CancellationToken cancellationToken)
    {
        request ??= new TRequest();

        await PreProcessRequest(request, cancellationToken);

        return await mediator.Send(request, cancellationToken);
    }

    protected virtual Task PreProcessRequest(TRequest request, CancellationToken cancellationToken)
    {
        return _requestPreProcessor?.Invoke(request, cancellationToken) ?? Task.CompletedTask;
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .DescriptionFrom(typeof(TRequest))
            .Produces<TResponse>();
    }
}
