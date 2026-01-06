using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class SimplePostMinimalApiBuilder<TRequest, TResponse>(string route)
    : BaseMediatrMinimalApiBuilder<TRequest, TResponse>(route)
    where TRequest : IRequest<TResponse>, new()
{
    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Post;
}

#pragma warning disable SA1402
public class SimplePostMinimalApiBuilder<TRequest>(string route) : BaseMediatrMinimalApiBuilder<TRequest>(route)
    where TRequest : IRequest, new()
{
    protected override string GetSchemaName()
    {
        return MinimalApiHelper.GetSchemaName(RequestType);
    }

    protected override string HttpMethod => HttpMethods.Post;
}
#pragma warning restore SA1402