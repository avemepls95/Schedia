using Avemepls.Domain.Exceptions;
using Avemepls.Mvc.Filters.Base;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters;

/// <summary>
/// Фильтр для исключения <see cref="AccessDeniedException"/>.
/// </summary>
public class AccessDeniedExceptionFilter : ExceptionFilter<AccessDeniedException>
{
    /// <inheritdoc />
    protected override void Handle(AccessDeniedException exception, ExceptionContext context)
    {
        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}