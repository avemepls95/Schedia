using Avemepls.Mvc.Errors;
using Avemepls.Mvc.Filters.Base;

namespace Avemepls.Mvc.Filters;

/// <summary>
/// Фильтр для исключения <see cref="ApplicationException"/>
/// </summary>
public class ApplicationExceptionFilter : BadRequestExceptionFilterBase<ApplicationException>
{
    internal override BadRequestErrorModel GetError(ApplicationException exception) => new BadRequestErrorModel
    {
        Message = exception.Message
    };
}