using System.Reflection;

using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class SearchRequestMinimalApiBuilder<TRequest, TModel>(string route) : MinimalApiBuilderBase(route)
    where TRequest : SearchQuery<TModel>
{
    protected static Type RequestType { get; } = typeof(TRequest);

#pragma warning disable S2743
    private static readonly PropertyInfo[] RequestArrayProperties =
        RequestType.GetProperties().Where(x => x.PropertyType.IsArray).ToArray();
#pragma warning restore S2743

    protected override string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(RequestType);
    }

    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Get;

    protected override Delegate GetDelegate() => Execute;

    private static Task<PagedResponse<TModel>> Execute(
        [AsParameters]
        TRequest request,
        [FromServices]
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // TO allow nullable arrays
        // https://github.com/dotnet/aspnetcore/issues/45956
        foreach (var param in RequestArrayProperties)
        {
            var value = param.GetValue(request);
            var isNullable = param.CustomAttributes.Any(x => x.AttributeType.Name == "NullableAttribute");

            if (value is Array { Length: 0 } && isNullable)
            {
                param.SetValue(request, null);
            }
        }

        return mediator.Send(request, cancellationToken);
    }

    protected override void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder
            .DescriptionFrom(RequestType)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .Produces<PagedResponse<TModel>>();
    }
}