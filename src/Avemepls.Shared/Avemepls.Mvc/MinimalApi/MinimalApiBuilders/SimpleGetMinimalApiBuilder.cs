using System.Reflection;

using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class SimpleGetMinimalApiBuilder<TRequest, TResponse>(string route)
    : BaseMediatrMinimalApiBuilder<TRequest, TResponse>(route)
    where TRequest : IRequest<TResponse>, new()
{
    private static readonly PropertyInfo[] NullableArrays =
        typeof(TRequest).GetProperties().Where(x => x.PropertyType.IsArray).ToArray();

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Get;

    protected override Delegate GetDelegate() => ExecuteInternal;

    private static async Task<TResponse> ExecuteInternal(
        [AsParameters] // Can't use FromQuery because need implementation TRequest.TryParse
        TRequest request,
        [FromServices]
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        FixNullableTypes(request);

        return await mediator.Send(request, cancellationToken);
    }

    private static void FixNullableTypes(TRequest request)
    {
        // TO allow nullable arrays
        // https://github.com/dotnet/aspnetcore/issues/45956
        foreach (var param in NullableArrays)
        {
            var value = param.GetValue(request);
            var isNullable = param.CustomAttributes.Any(x => x.AttributeType.Name == "NullableAttribute");

            if (value is Array { Length: 0 } && isNullable)
            {
                param.SetValue(request, null);
            }
        }
    }
}