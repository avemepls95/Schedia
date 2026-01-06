using System.Diagnostics.CodeAnalysis;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

[ExcludeFromCodeCoverage]
public abstract class MinimalApiBuilderBase(string route)
{
    private static readonly ExecuteMediatrRequestFilter ExecuteMediatrRequestFilter = new();
    private string? _actionName;
    private string? _overridenRoute;
    private string? _httpMethod;
    private bool _withAuthorization;
    private string? _schemaName;

    private Action<RouteHandlerBuilder>? Configuration { get; set; }

    private Delegate? _mapper;

    public MinimalApiBuilderBase WithMapper(Delegate mapper)
    {
        _mapper = mapper;

        return this;
    }

    private string[]? Permissions { get; set; }

    public MinimalApiBuilderBase WithPermissions(params string[] permissions)
    {
        Permissions = permissions;

        return this;
    }

    public MinimalApiBuilderBase WithAuthorization()
    {
        _withAuthorization = true;

        return this;
    }

    /// <summary>
    /// Will set groupName for building separate schemas
    /// Default value is first part of namespace
    /// </summary>
    public MinimalApiBuilderBase WithSchemaName(string schemaName)
    {
        _schemaName = schemaName;

        return this;
    }

    public MinimalApiBuilderBase WithActionName(string name)
    {
        _actionName = name;

        return this;
    }

    public MinimalApiBuilderBase WithRoute(string route)
    {
        _overridenRoute = route;

        return this;
    }

    public MinimalApiBuilderBase WithHttpMethod(string httpMethod)
    {
        _httpMethod = httpMethod;

        return this;
    }

    public MinimalApiBuilderBase Configure(Action<RouteHandlerBuilder> configure)
    {
        Configuration = configure;

        return this;
    }

    protected abstract string GetGroupName();
    protected abstract string GetSchemaName();

    protected abstract Delegate GetDelegate();

    protected abstract string HttpMethod { get; }

    protected virtual void ConfigureInternal(RouteHandlerBuilder routeHandlerBuilder)
    {
    }

    internal virtual void Build(IEndpointRouteBuilder app)
    {
        var groupName = GetGroupName();

        var route = GetRoute();

        var routeBuilder = app
            .MapMethods(route, new[] { _httpMethod ?? HttpMethod }, _mapper ?? GetDelegate())
            .WithTagsIfNotExists(groupName);

        routeBuilder.UseBaseFilters(app.ServiceProvider);

        if (_mapper is not null)
        {
            routeBuilder.AddEndpointFilter(ExecuteMediatrRequestFilter);
        }

        ConfigureInternal(routeBuilder);

        routeBuilder.WithGroupNameIfNotExists(_schemaName ?? GetSchemaName());

        if (_actionName is not null)
        {
            routeBuilder.WithName(_actionName);
        }

        Configuration?.Invoke(routeBuilder);

        if (Permissions is not null || _withAuthorization)
        {
            routeBuilder
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            if (_withAuthorization)
            {
                routeBuilder.RequireAuthorization();
            }

            if (Permissions is not null)
            {
                foreach (var permission in Permissions)
                {
                    routeBuilder
                        .RequireAuthorization(new AuthorizeAttribute(permission)
                        {
                            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                        });
                }
            }
        }
    }

    protected string GetRoute()
    {
        var route1 = route;

        if (_overridenRoute is not null)
        {
            if (_overridenRoute.StartsWith('/'))
            {
                route1 = _overridenRoute;
            }
            else
            {
                route1 += "/" + _overridenRoute;
            }
        }

        return route1;
    }
}