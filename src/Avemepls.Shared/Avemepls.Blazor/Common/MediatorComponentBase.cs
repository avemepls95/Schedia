using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Avemepls.Blazor.Common;

public abstract class MediatorComponentBase : ComponentBase, IDisposable
{
    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    [Inject]
    protected IMediator Mediator { get; set; } = null!;

    [Inject]
    protected IStringLocalizerFactory StringLocalizerFactory { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();

    protected ILogger Logger => field ??= LoggerFactory.CreateLogger(GetType());

    protected IStringLocalizer Loc => field ??= StringLocalizerFactory.Create(GetType());

    protected CancellationToken CancellationToken => _cts.Token;

    protected string ErrorMessage { get; set; } = string.Empty;
    protected string AdditionalInformationAsWarning { get; set; } = string.Empty;

    public delegate void ErrorHandler();

    protected event ErrorHandler ErrorRaised = OnErrorRaised;

    private static void OnErrorRaised()
    {
        // Method intentionally left empty.
    }

    protected async Task<TResponse?> Send<TResponse>(IRequest<TResponse> request)
        where TResponse : class
    {
        return (TResponse?)await Send(request as IBaseRequest);
    }

    protected async Task Send(IRequest request)
    {
        await Send(request as IBaseRequest);
    }

    private async Task<object?> Send(IBaseRequest baseRequest)
    {
        try
        {
            return await Mediator.Send(baseRequest, CancellationToken);
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message;
            ErrorRaised.Invoke();
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            Logger.LogError(ex, "Unhandled exception in {Component}. ErrorId: {ErrorId}", GetType().Name, errorId);

            ErrorMessage = Loc["Возникла непредвиденная ошибка"];
            AdditionalInformationAsWarning = Loc["Чтобы мы скорее решили эту проблему, сообщите пожалуйста нашей команде технической поддержки код ошибки {0}", errorId];

            ErrorRaised.Invoke();
        }

        return null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}