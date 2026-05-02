using System.Collections.Concurrent;
using System.Reflection;

using Avemepls.Core.Models;
using Avemepls.ServiceBus.Common;
using Avemepls.ServiceBus.Exceptions;
using Avemepls.ServiceBus.Helpers;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Avemepls.ServiceBus.Requests;

/// <summary>
/// Медиатор для межпроцессорного взаимодействия
/// </summary>
public interface ICrossDomainMediator
{
    ValueTask<TResponse> Send<TResponse>(
        ICrossDomainRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : class, new();

    Task<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class, ICrossDomainRequest<TResponse>, new()
        where TResponse : class, new();

    ValueTask Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class, ICrossDomainCommand, new();
}

internal sealed class CrossDomainMediator : ICrossDomainMediator
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> SendRequestsCache = new();

    private readonly IClientFactory _clientFactory;
    private readonly ILogger<CrossDomainMediator> _logger;

    public CrossDomainMediator(
        IClientFactory clientFactory,
        ISendEndpointProvider sendEndpointProvider,
        ILogger<CrossDomainMediator> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async ValueTask<TResponse> Send<TResponse>(
        ICrossDomainRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : class, new()
    {
        var requestType = request.GetType();

        var method = SendRequestsCache.GetOrAdd(requestType,
                                                 type => typeof(CrossDomainMediator).GetMethods()
                                                     .First(x => x.IsGenericMethod &&
                                                                 x.GetGenericArguments().Length == 2 &&
                                                                 x.Name == nameof(Send))
                                                     ?.MakeGenericMethod(type, typeof(TResponse))!);

        if (method == null)
        {
            throw new InvalidOperationException($"Could not find Send method from {typeof(CrossDomainMediator)}.");
        }

        var requestHandler = (Task)method.Invoke(this, [request, cancellationToken])!;

        await requestHandler.ConfigureAwait(false);

        var responseProperty = requestHandler.GetType().GetProperty("Result");
        var response = (TResponse)responseProperty!.GetValue(requestHandler)!;

        return response;
    }

    public async Task<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class, ICrossDomainRequest<TResponse>, new()
        where TResponse : class, new()
    {
        var uri = new Uri("queue:" + EndpointHelper.GenerateEndpointName(typeof(TRequest)));

        var client = _clientFactory.CreateRequestClient<TRequest>(uri);

        try
        {
            var response = await client.GetResponse<TResponse, CrossDomainRequestError>(request, cancellationToken);

            if (response.Is(out Response<TResponse>? result))
            {
                return result.Message;
            }

            if (response.Is(out Response<CrossDomainRequestError>? errorResponse))
            {
                _logger.LogError("Error while sending request {RequestType}", typeof(TRequest));

                throw new CrossDomainRequestException(errorResponse.Message.Message,
                                                      errorResponse.Message.Details,
                                                      errorResponse.Message.SourceExceptionTypeName);
            }

            throw new CrossDomainRequestException(
                $"Can't handle response for {typeof(TRequest)}: {@response.Message}.");
        }
        catch (RequestTimeoutException)
        {
            throw new CrossDomainRequestException($"The server is not responding for {request.GetType()}");
        }
    }

    public async ValueTask Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class, ICrossDomainCommand, new()
    {
        var uri = new Uri("queue:" + EndpointHelper.GenerateEndpointName(typeof(TRequest)));

        var client = _clientFactory.CreateRequestClient<TRequest>(uri);

        try
        {
            var response = await client.GetResponse<CrossDomainCommandSuccess, CrossDomainRequestError>(request, cancellationToken);

            if (response.Is(out Response<CrossDomainCommandSuccess>? _))
            {
                return;
            }

            if (response.Is(out Response<CrossDomainRequestError>? errorResponse))
            {
                _logger.LogError("Error while sending request {RequestType}", typeof(TRequest));

                throw new CrossDomainRequestException(errorResponse.Message.Message,
                                                      errorResponse.Message.Details,
                                                      errorResponse.Message.SourceExceptionTypeName);
            }

            throw new CrossDomainRequestException(
                $"Can't handle response for {typeof(TRequest)}: {@response.Message}.");
        }
        catch (RequestTimeoutException)
        {
            throw new CrossDomainRequestException($"The server is not responding for {request.GetType()}");
        }
    }
}