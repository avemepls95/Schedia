using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Avemepls.Core.Models;
using Avemepls.Mvc.MinimalApi.Middleware;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Avemepls.Mvc.MinimalApi;

/// <summary>
/// Extensions
/// </summary>
public static partial class MinimalApiExtensions
{
    private const string ApiPrefix = "api";

    public static IEndpointRouteBuilder RegisterMinimalApiControllers(
        this IEndpointRouteBuilder app,
        params Assembly[] assemblies)
    {
        var controllers = assemblies
            .SelectMany(x => x.DefinedTypes)
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(ControllerBase)))
            .ToArray();

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var registerMethod = Array.Find(methods,
                                            x => x.Name == "Register" &&
                                                 Array.Exists(x.GetParameters(),
                                                              w => w.ParameterType == typeof(IEndpointRouteBuilder)));

            if (registerMethod is not null)
            {
                registerMethod.Invoke(null, new object[] { app });
            }
        }

        return app;
    }

    public static RouteHandlerBuilder ProduceUserConfirmations<TExample>(this RouteHandlerBuilder routeHandlerBuilder)
        where TExample : IMultipleExamplesProvider<UserConfirmationItem[]>
    {
        return routeHandlerBuilder.ProduceUserConfirmations()
            .WithExample<TExample, UserConfirmationItem[]>((int)HttpStatusCode.Conflict);
    }

    public static RouteHandlerBuilder ProduceUserConfirmations(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder.Produces<UserConfirmationItem[]>(StatusCodes.Status409Conflict);
    }

    public static RouteHandlerBuilder WithExample<TExample, TResponse>(
        this RouteHandlerBuilder routeHandlerBuilder,
        int statusCode)
        where TExample : IMultipleExamplesProvider<TResponse>
    {
        return routeHandlerBuilder.WithMetadata(new SwaggerResponseExampleAttribute(statusCode, typeof(TExample)));
    }

    public static RouteHandlerBuilder UseBaseFilters(this RouteHandlerBuilder app, IServiceProvider serviceProvider)
    {
        var routeHandlerBuilder = app
            .AddEndpointFilter(MinimalApiFilters.ApplicationExceptionEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.UserConfirmationContextEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.OperationCancelledEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.ObjectNotFoundEndpointFilterInt)
            .AddEndpointFilter(MinimalApiFilters.ListValidationExceptionEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.ObjectNotFoundEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.ValidationEndpointFilter)
            .AddEndpointFilter(MinimalApiFilters.ConcurrencyEndpointFilterInt)
            .AddEndpointFilter(MinimalApiFilters.AccessDeniedExceptionEndpointFilter);

        var exceptionFilters = serviceProvider.GetRequiredService<IEnumerable<IEndpointFilter>>();

        foreach (var filter in exceptionFilters)
            routeHandlerBuilder = routeHandlerBuilder.AddEndpointFilter(filter);

        return routeHandlerBuilder;
    }

    /// <summary>
    /// Зарегистрировать обработку команды или запроса
    /// </summary>
    public static IEndpointRouteBuilder MapMediatrRequest<TRequest, TResponse>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.BaseMediatrMinimalApiBuilder<TRequest, TResponse>>? configure = null)
        where TRequest : IRequest<TResponse>, new()
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.SimplePostMinimalApiBuilder<TRequest, TResponse>(baseRoute);
        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    /// <summary>
    /// Зарегистрировать обработку команды или запроса
    /// </summary>
    public static IEndpointRouteBuilder MapMediatrRequest<TRequest>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.BaseMediatrMinimalApiBuilder<TRequest>>? configure = null)
        where TRequest : IRequest, new()
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);

        var builder = new MinimalApiBuilders.SimplePostMinimalApiBuilder<TRequest>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapGetQuery<TRequest, TResponse>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.SimpleGetMinimalApiBuilder<TRequest, TResponse>>? configure = null)
        where TRequest : IRequest<TResponse>, new()
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.SimpleGetMinimalApiBuilder<TRequest, TResponse>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapGetByIdQuery<TRequest, TModel, TId>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.GetByIdQueryMinimalApiBuilder<TRequest, TModel, TId>>? configure = null)
        where TRequest : GetEntityByIdQuery<TModel, TId>
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.GetByIdQueryMinimalApiBuilder<TRequest, TModel, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapGetByIdQuery<TRequest, TModel>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.GetByIdQueryMinimalApiBuilder<TRequest, TModel, int>>? configure = null)
        where TRequest : GetEntityByIdQuery<TModel, int>
    {
        return app.MapGetByIdQuery<TRequest, TModel, int>(configure);
    }

    public static IEndpointRouteBuilder MapDeleteCommand<TEntity, TId>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.DeleteMinimalApiBuilder<TEntity, TId>>? configure = null)
        where TEntity : IHasId<TId>, new()
    {
        var routeName = BuildBaseRouteNameFromType<TEntity>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.DeleteMinimalApiBuilder<TEntity, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapDeleteCommand<TEntity>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.DeleteMinimalApiBuilder<TEntity, int>>? configure = null)
        where TEntity : IHasId<int>, new()
    {
        return app.MapDeleteCommand<TEntity, int>(configure);
    }

    public static IEndpointRouteBuilder MapRestoreCommand<TEntity, TId>(
      this IEndpointRouteBuilder app,
      Action<MinimalApiBuilders.RestoreMinimalApiBuilder<TEntity, TId>>? configure = null)
      where TEntity : IHasId<TId>, IHasDateDeleted, new()
    {
        var routeName = BuildBaseRouteNameFromType<TEntity>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.RestoreMinimalApiBuilder<TEntity, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapRestoreCommand<TEntity>(
      this IEndpointRouteBuilder app,
      Action<MinimalApiBuilders.RestoreMinimalApiBuilder<TEntity, int>>? configure = null)
      where TEntity : IHasId<int>, IHasDateDeleted, new()
    {
        return app.MapRestoreCommand<TEntity, int>(configure);
    }

    public static IEndpointRouteBuilder MapCreateCommand<TRequest, TId>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.CreateCommandMinimalApiBuilder<TRequest, TId>>? configure = null)
        where TRequest : ICreateCommand, IRequest<TId>, new()
        where TId : struct
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.CreateCommandMinimalApiBuilder<TRequest, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapCreateCommand<TRequest>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.CreateCommandMinimalApiBuilder<TRequest, int>>? configure = null)
        where TRequest : ICreateCommand, IRequest<int>, new()
    {
        return app.MapCreateCommand<TRequest, int>(configure);
    }

    public static IEndpointRouteBuilder MapUpdateCommand<TRequest, TId>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.UpdateCommandMinimalApiBuilder<TRequest, TId>>? configure = null)
        where TRequest : IUpdateCommand<TId>, IRequest<TId>, new()
        where TId : struct
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.UpdateCommandMinimalApiBuilder<TRequest, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapUpdateCommand<TRequest>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.UpdateCommandMinimalApiBuilder<TRequest, int>>? configure = null)
        where TRequest : IUpdateCommand<int>, IRequest<int>, new()
    {
        return app.MapUpdateCommand<TRequest, int>(configure);
    }

    public static IEndpointRouteBuilder MapCreateUpdateCommand<TRequest, TModel, TId>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId>>? configure = null)
        where TRequest : CreateUpdateCommand<TId, TModel>
        where TId : struct
        where TModel : notnull
    {
        var routeName = BuildBaseRouteNameFromType<TRequest>();
        var baseRoute = app.GetBaseRoute(routeName);
        var builder = new MinimalApiBuilders.CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, TId>(baseRoute);

        configure?.Invoke(builder);
        builder.Build(app);

        return app;
    }

    public static IEndpointRouteBuilder MapCreateUpdateCommand<TRequest, TModel>(
        this IEndpointRouteBuilder app,
        Action<MinimalApiBuilders.CreateUpdateCommandMinimalApiBuilder<TRequest, TModel, int>>? configure = null)
        where TRequest : CreateUpdateCommand<int, TModel>
        where TModel : notnull
    {
        return app.MapCreateUpdateCommand<TRequest, TModel, int>(configure);
    }

    /// <summary>
    /// Вложенный маппинг для контроллера
    /// </summary>
    public static RouteGroupBuilder MapGroup(
        this IEndpointRouteBuilder app,
        Assembly assembly,
        string groupRoute)
    {
        var baseNamespaceRouteName = BuildBaseRouteNameFromAssembly(assembly);

        var route = $"{ApiPrefix}/{baseNamespaceRouteName}/{groupRoute}";

        route = KebabCaseRegex().Replace(route, "$1-$2").ToLower(CultureInfo.InvariantCulture);

        return app.MapGroup(route);
    }

    /// <summary>
    /// Регистрация всем фильтров в сборке
    /// </summary>
    public static IServiceCollection AddEndpointFilters(this IServiceCollection services, params Assembly[] assemblies)
    {
        var endpointFilterTypes = assemblies.SelectMany(assembly => assembly.GetTypes()
                                                             .Where(x => typeof(IEndpointFilter).GetTypeInfo()
                                                                             .IsAssignableFrom(x.GetTypeInfo()) &&
                                                                         x.GetTypeInfo().IsClass &&
                                                                         !x.GetTypeInfo().IsAbstract))
            .ToList();

        foreach (var endpointFilterType in endpointFilterTypes)
        {
            services.TryAddEnumerable(
                new ServiceDescriptor(typeof(IEndpointFilter), endpointFilterType, ServiceLifetime.Transient));
        }

        return services;
    }

    private static string GetBaseRoute(this IEndpointRouteBuilder app, string routeName)
    {
        if (app is RouteGroupBuilder)
        {
            return string.Empty;
        }

        return $"{ApiPrefix}/{routeName}";
    }

    private static string BuildBaseRouteNameFromType<TRequest>()
    {
        var nameSpace = typeof(TRequest).Namespace!.Split('.')[1];

        return KebabCaseRegex().Replace(nameSpace, "$1-$2").ToLower(CultureInfo.InvariantCulture);
    }

    private static string BuildBaseRouteNameFromAssembly(Assembly assembly)
    {
        var nameSpace = assembly.GetName().FullName.Split('.')[1];

        return KebabCaseRegex().Replace(nameSpace, "$1-$2").ToLower(CultureInfo.InvariantCulture);
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex KebabCaseRegex();
}