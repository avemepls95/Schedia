namespace Avemepls.Mvc.MinimalApi.Middleware.Base;

/// <summary>
/// Базовый типизированный обработчик исключения.
/// </summary>
/// <typeparam name="TException">Тип исключения, который умеет обрабатывать фильтр.</typeparam>
public abstract class ExceptionEndpointFilterBase<TException> : IEndpointFilter
    where TException : Exception
{
    /// <summary>
    /// Метод, который нужно переопределить, чтобы обработать исключение.
    /// </summary>
    protected abstract IResult Handle(TException exception);

    /// <summary>
    /// Дополнительная проверка.
    /// </summary>
    protected virtual bool IsSatisfied(TException e) => true;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (TException e) when (IsSatisfied(e))
        {
            ModifyResponseHeaders(context.HttpContext.Response.Headers);

            return Handle(e);
        }
    }

    protected virtual void ModifyResponseHeaders(IHeaderDictionary headerDictionary)
    {
    }
}