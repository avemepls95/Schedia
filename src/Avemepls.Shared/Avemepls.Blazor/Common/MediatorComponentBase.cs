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

    protected abstract void HandleError(string message);

    protected async Task<TResponse?> Send<TResponse>(IRequest<TResponse> request)
    {
        try
        {
            return await Mediator.Send(request, CancellationToken);
        }
        catch (ValidationException ex)
        {
            HandleError(ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message);
            return default;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception in {Component}", GetType().Name);
            HandleError(Loc["Произошла непредвиденная ошибка"]);
            return default;
        }
    }

    protected async Task<bool> Send(IRequest request)
    {
        try
        {
            await Mediator.Send(request, CancellationToken);
            return true;
        }
        catch (ValidationException ex)
        {
            HandleError(ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception in {Component}", GetType().Name);
            HandleError(Loc["Произошла непредвиденная ошибка"]);
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}