using System.Net.Mime;

using Avemepls.Mvc.Errors;
using Avemepls.Mvc.MinimalApi.Middleware;
using Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders;

public class CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId>(string routeName)
    where TRequest : CreateUpdateCommand<TId, TModel>
    where TId : struct
    where TModel : notnull
{
    protected static Type RequestType { get; } = typeof(TRequest);

    private string[]? _createPermissions;
    private bool _withAuthorization;
    private string? _schemaName;
    private string? _entityName;

    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithAuthorization()
    {
        _withAuthorization = true;

        return this;
    }

    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithCreatePermissions(
        params string[] createPermissions)
    {
        _createPermissions = createPermissions;

        return this;
    }

    private string[]? _updatePermissions;

    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithUpdatePermissions(
        params string[] updatePermissions)
    {
        _updatePermissions = updatePermissions;

        return this;
    }

    private string? _overridenRoute;

    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithRoute(string route)
    {
        _overridenRoute = route;

        return this;
    }

    internal void Build(IEndpointRouteBuilder app)
    {
        BuildCreate(app);
        BuildUpdate(app);
    }

    /// <summary>
    /// Will set groupName for building separate schemas
    /// Default value is first part of namespace
    /// </summary>
    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithSchemaName(string schemaName)
    {
        _schemaName = schemaName;

        return this;
    }

    public CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId> WithEntityName(string name)
    {
        _entityName = name;

        return this;
    }

    private (Type GetByIdQyery, Type GetByIdDetailedModel)? _createdAtRoute;

    public void WithCreatedAt<TQuery, TDetailedModel>()
        where TQuery : IGetByIdQuery<TId>
    {
        _createdAtRoute = (typeof(TQuery), typeof(TDetailedModel));
    }

    private static string GetGroupName()
    {
        return MinimalApiHelper.GetGroupName(RequestType);
    }

    private async Task<IResult> ExecuteCreate([FromBody] TModel model, [FromServices] IMediator mediator)
    {
#pragma warning disable S3878
        var id = await mediator.Send((TRequest)Activator.CreateInstance(RequestType, args: new object?[] { model })!);
#pragma warning restore S3878

        if (_createdAtRoute is not null)
        {
            var query = Activator.CreateInstance(_createdAtRoute.Value.GetByIdQyery, id);
            var detailedModel = await mediator.Send(query!);

            return Results.Created($"{routeName}/{id}", detailedModel);
        }

        return Results.Ok(id);
    }

    private string GetRoute()
    {
        var route = routeName;

        if (_overridenRoute is not null)
        {
            if (_overridenRoute.StartsWith('/'))
            {
                route = _overridenRoute;
            }
            else
            {
                route += "/" + _overridenRoute;
            }
        }

        return route;
    }

    private void BuildCreate(IEndpointRouteBuilder app)
    {
        var groupName = GetGroupName();
        var type = RequestType;
        type = type.ReflectedType ?? type;
        var endpointName = !string.IsNullOrEmpty(_entityName)
                           ? "Create" + _entityName
                            : type.Name.Replace("CreateUpdate", "Create");

        var routeBuilder = app
            .MapPost(GetRoute(), ExecuteCreate)
            .WithTagsIfNotExists(groupName)
            .Accepts<TModel>(MediaTypeNames.Application.Json)
            .DescriptionFrom(RequestType)
            .UseBaseFilters(app.ServiceProvider)
            .WithGroupNameIfNotExists(_schemaName ?? MinimalApiHelper.GetSchemaName(RequestType))
            .WithName(endpointName)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter(MinimalApiFilters.ValidationEndpointFilter);

        if (_createdAtRoute is not null)
        {
            routeBuilder.Produces(StatusCodes.Status200OK, _createdAtRoute.Value.GetByIdDetailedModel);
        }
        else
        {
            routeBuilder.Produces(StatusCodes.Status200OK, typeof(TId));
        }

        if (_createPermissions is not null || _withAuthorization)
        {
            routeBuilder
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            if (_withAuthorization)
            {
                routeBuilder.RequireAuthorization();
            }

            if (_createPermissions is not null)
            {
                foreach (var permission in _createPermissions)
                {
                    routeBuilder.RequireAuthorization(new AuthorizeAttribute(permission)
                    {
                        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                    });
                }
            }
        }
    }

    private static async Task ExecuteUpdate(
        [FromRoute]
        TId id,
        [FromBody]
        TModel model,
        [FromServices]
        IMediator mediator)
    {
#pragma warning disable S3878
        await mediator.Send((TRequest)Activator.CreateInstance(typeof(TRequest), args: new object?[] { id, model })!);
#pragma warning restore S3878
    }

    private void BuildUpdate(IEndpointRouteBuilder app)
    {
        var groupName = GetGroupName();
        var type = RequestType;
        type = type.ReflectedType ?? type;
        var endpointName = !string.IsNullOrEmpty(_entityName)
            ? "Update" + _entityName
            : type.Name.Replace("CreateUpdate", "Update");

        var routeBuilder = app
            .MapPut(GetRoute() + "/{id}", ExecuteUpdate)
            .WithTagsIfNotExists(groupName)
            .AddEndpointFilter(MinimalApiFilters.ValidationEndpointFilter)
            .DescriptionFrom(RequestType)
            .UseBaseFilters(app.ServiceProvider)
            .WithGroupNameIfNotExists(_schemaName ?? MinimalApiHelper.GetSchemaName(RequestType))
            .WithName(endpointName)
            .Produces<BadRequestErrorModel>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        if (_updatePermissions is not null || _withAuthorization)
        {
            routeBuilder
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            if (_withAuthorization)
            {
                routeBuilder.RequireAuthorization();
            }

            if (_updatePermissions is not null)
            {
                foreach (var permission in _updatePermissions)
                {
                    routeBuilder.RequireAuthorization(new AuthorizeAttribute(permission)
                    {
                        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                    });
                }
            }
        }
    }
}