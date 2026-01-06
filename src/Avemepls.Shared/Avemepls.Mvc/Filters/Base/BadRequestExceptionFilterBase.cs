using Avemepls.Mvc.Errors;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters.Base;

public abstract class BadRequestExceptionFilterBase<TException> : ExceptionFilter<TException>
    where TException : Exception
{
    protected override void Handle(TException exception, ExceptionContext context)
    {
        var errorModel = GetError(exception);
        context.Result = new BadRequestObjectResult(errorModel);
    }

    /// <summary>
    /// Создание модели ошибки из исключения
    /// </summary>
    internal abstract BadRequestErrorModel GetError(TException exception);
}