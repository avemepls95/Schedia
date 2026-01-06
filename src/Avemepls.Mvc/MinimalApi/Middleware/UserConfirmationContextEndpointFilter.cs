using System.Text.Json;
using System.Text.Json.Serialization;

using Avemepls.Mvc.MinimalApi.Middleware.Base;

namespace Avemepls.Mvc.MinimalApi.Middleware;

public class UserConfirmationContextEndpointFilter : ExceptionEndpointFilterBase<UserConfirmationContextException>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected override IResult Handle(UserConfirmationContextException exception)
    {
        return Results.Json(exception.Items, JsonSerializerOptions, statusCode: StatusCodes.Status409Conflict);
    }

    protected override void ModifyResponseHeaders(IHeaderDictionary headerDictionary)
    {
        headerDictionary["Confirmation-Required"] = "true";
    }
}