using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters.Base;

/// <summary>
/// Базовый типизированный обработчик исключения.
/// </summary>
/// <typeparam name="TException">Тип исключения, который умеет обрабатывать фильтр.</typeparam>
public abstract class ExceptionFilter<TException> : IExceptionFilter, IAsyncExceptionFilter
    where TException : Exception
{
    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is TException e && IsSatisfied(e))
        {
            Handle(e, context);
            context.ExceptionHandled = true;
        }
    }

    /// <inheritdoc />
    public Task OnExceptionAsync(ExceptionContext context)
    {
        OnException(context);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Метод, который нужно переопределить, чтобы обработать исключение.
    /// </summary>
    protected abstract void Handle(TException exception, ExceptionContext context);

    /// <summary>
    /// Дополнительная проверка.
    /// </summary>
    protected virtual bool IsSatisfied(TException e) => true;
}