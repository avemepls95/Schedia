using Avemepls.Domain.Exceptions;
using Avemepls.Mvc.Filters.Base;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters;

/// <summary>
/// Обрабатывает ошибки параллельного обновления.
/// </summary>
public class ConcurrencyExceptionFilter<T> : ExceptionFilter<ConcurrencyException<T>>
{
    /// <inheritdoc />
    protected override void Handle(ConcurrencyException<T> exception, ExceptionContext context)
    {
        context.Result = new ConflictResult();
    }
}