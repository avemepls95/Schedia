using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

#pragma warning disable SA1402

/// <summary>
/// Отлавливает исключение о ненайденном объекте.
/// </summary>
public sealed class ObjectNotFoundEndpointFilter<T> : ExceptionEndpointFilterBase<ObjectNotFoundException<T>>
{
    protected override IResult Handle(ObjectNotFoundException<T> exception)
    {
        return Results.NotFound(exception.Message);
    }
}

/// <summary>
/// Отлавливает исключение о ненайденном объекте.
/// </summary>
public sealed class ObjectNotFoundEndpointFilter : ExceptionEndpointFilterBase<ObjectNotFoundException>
{
    protected override IResult Handle(ObjectNotFoundException exception)
    {
        return Results.NotFound(exception.Message);
    }
}
#pragma warning restore SA1402