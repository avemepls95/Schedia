using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

/// <summary>
/// Обрабатывает ошибки параллельного обновления.
/// </summary>
public class ConcurrencyEndpointFilter<T> : ExceptionEndpointFilterBase<ConcurrencyException<T>>
{
    protected override IResult Handle(ConcurrencyException<T> exception)
    {
        return Results.Conflict();
    }
}