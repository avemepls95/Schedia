using Avemepls.Domain.Exceptions;
using Avemepls.Mvc.Filters.Base;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters;

/// <summary>
/// Отлавливает исключение о ненайденном объекте.
/// </summary>
public class ObjectNotFoundExceptionFilter<T> : ExceptionFilter<ObjectNotFoundException<T>>
    where T : class
{
    /// <inheritdoc />
    protected override void Handle(ObjectNotFoundException<T> exception, ExceptionContext context)
    {
        context.Result = new NotFoundResult();
    }
}