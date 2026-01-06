using Avemepls.Mvc.Filters.Base;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avemepls.Mvc.Filters;

public class OperationCancelledExceptionFilter : ExceptionFilter<OperationCanceledException>
{
    protected override void Handle(OperationCanceledException exception, ExceptionContext context)
    {
        context.Result = new StatusCodeResult(499);
    }
}