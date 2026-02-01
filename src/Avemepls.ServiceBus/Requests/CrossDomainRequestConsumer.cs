using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.ServiceBus.Common;

using MassTransit;

using MediatR;

namespace Avemepls.ServiceBus.Requests;

#pragma warning disable SA1402

/// <summary>
/// Базовая реализация обработки межсервисных запросов через шину
/// </summary>
public abstract class CrossDomainRequestHandlerBase<TRequest> : IConsumer<TRequest>
    where TRequest : class
{
    public async Task Consume(ConsumeContext<TRequest> context)
    {
        try
        {
            var result = await Execute(context.Message, context.CancellationToken);

            await HandleResult(context, result);
        }
        catch (Exception e)
        {
            await context.RespondAsync(new CrossDomainRequestError(e));
        }
    }

    protected virtual async Task HandleResult(ConsumeContext<TRequest> context, object? result)
    {
        if (result is not null)
        {
            await context.RespondAsync(result, result.GetType());
        }
        else
        {
            await context.RespondAsync(new CrossDomainCommandSuccess());
        }
    }

    protected abstract ValueTask<object?> Execute(TRequest message, CancellationToken cancellationToken);
}

/// <inheritdoc />
public abstract class CrossDomainRequestHandlerBase<TCrossDomainRequest, TCrossDomainResponse>(ISender mediator) :
    CrossDomainRequestHandlerBase<TCrossDomainRequest>
    where TCrossDomainRequest : class
    where TCrossDomainResponse : class, new()
{
    protected override async ValueTask<object?> Execute(
        TCrossDomainRequest message,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(message);

        var response = await mediator.Send(request, cancellationToken);

        return MapResponse(response);
    }

    protected abstract TCrossDomainResponse MapResponse(object? response);

    protected abstract IBaseRequest MapRequest(TCrossDomainRequest request);

    protected override async Task HandleResult(ConsumeContext<TCrossDomainRequest> context, object? result)
    {
        if (result is null)
        {
            await context.RespondAsync(new CrossDomainRequestError("No result received"));
        }

        await base.HandleResult(context, result);
    }
}

/// <inheritdoc />
public abstract class CrossDomainRequestHandler<TCrossDomainRequest, TCrossDomainResponse>(ISender mediator) :
    CrossDomainRequestHandlerBase<TCrossDomainRequest, TCrossDomainResponse>(mediator)
    where TCrossDomainRequest : class, ICrossDomainRequest<TCrossDomainResponse>, IRequest<TCrossDomainResponse>, new()
    where TCrossDomainResponse : class, new()
{
    private readonly ISender _mediator = mediator;

    protected override async ValueTask<object?> Execute(
        TCrossDomainRequest message,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(message);

        var response = await _mediator.Send(request, cancellationToken);

        return MapResponse(response);
    }
}

/// <inheritdoc />
public abstract class CrossDomainRequestHandler<TCrossDomainRequest, TCrossDomainResponse, TDomainRequest>(ISender mediator, IMapper mapper) :
    CrossDomainRequestHandlerBase<TCrossDomainRequest, TCrossDomainResponse>(mediator)
    where TDomainRequest : IRequest<TCrossDomainResponse>, new()
    where TCrossDomainResponse : class, new()
    where TCrossDomainRequest : class, ICrossDomainRequest<TCrossDomainResponse>, new()
{
    private readonly ISender _mediator = mediator;

    protected override async ValueTask<object?> Execute(
        TCrossDomainRequest message,
        CancellationToken cancellationToken)
    {
        var request = mapper.Map<TDomainRequest>(message);
        var response = await _mediator.Send(request, cancellationToken);

        return response;
    }

    protected override TCrossDomainResponse MapResponse(object? response)
    {
        return mapper.Map<TCrossDomainResponse>(response!);
    }
}

/// <inheritdoc />
public abstract class CrossDomainRequestHandler<
    TCrossDomainRequest,
    TCrossDomainResponse,
    TDomainRequest,
    TDomainResponse>(ISender mediator, IMapper mapper) : CrossDomainRequestHandlerBase<TCrossDomainRequest, TCrossDomainResponse>(mediator)
    where TCrossDomainRequest : class, ICrossDomainRequest<TCrossDomainResponse>, new()
    where TCrossDomainResponse : class, new()
    where TDomainRequest : IRequest<TDomainResponse>
{
    protected override IBaseRequest MapRequest(TCrossDomainRequest request)
    {
        return mapper.Map<TDomainRequest>(request);
    }

    protected override TCrossDomainResponse MapResponse(object? response)
    {
        return mapper.Map<TCrossDomainResponse>(response!);
    }

    protected override async Task HandleResult(ConsumeContext<TCrossDomainRequest> context, object? result)
    {
        if (result is null)
        {
            await context.RespondAsync(new CrossDomainRequestError("No result received"));
        }

        await base.HandleResult(context, result);
    }
}

/// <inheritdoc />
public abstract class CrossDomainRequestHandler<TCrossDomainRequest>(ISender mediator) :
    CrossDomainRequestHandlerBase<TCrossDomainRequest>
    where TCrossDomainRequest : class, ICrossDomainCommand, new()
{
    protected override async ValueTask<object?> Execute(
        TCrossDomainRequest message,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(message);

        await mediator.Send(request, cancellationToken);

        return null;
    }

    protected abstract IBaseRequest MapRequest(TCrossDomainRequest request);
}
#pragma warning restore SA1402