using System.Globalization;

namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

internal static class MinimalApiHelper
{
    internal static string GetGroupName(Type requestType)
    {
        var groupName = "default";
        var partsOfNamespace = requestType.Namespace!.Split('.');

        if (partsOfNamespace.Length > 1)
        {
            groupName = partsOfNamespace[1];
        }

        return groupName;
    }

    internal static string GetSchemaName(Type requestType)
    {
        var groupName = "default";
        var partsOfNamespace = requestType.Namespace!.Split('.').Where(x => x != nameof(Quirco)).ToArray();

        if (partsOfNamespace.Length > 0)
        {
            groupName = partsOfNamespace[0];
        }

        return groupName.ToLower(CultureInfo.InvariantCulture);
    }

    internal static RouteHandlerBuilder DescriptionFrom<TRequest>(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder.DescriptionFrom(typeof(TRequest));
    }

    internal static RouteHandlerBuilder DescriptionFrom(this RouteHandlerBuilder routeHandlerBuilder, Type type)
    {
        var name = type.Name;

        if (type is { IsNested: true, ReflectedType: not null })
        {
            type = type.ReflectedType;
            name = type.Name;
        }

        return routeHandlerBuilder.WithSummary(type!.GetXmlDocsSummary()).WithName(name).WithOpenApi();
    }

    internal static RouteHandlerBuilder WithGroupNameIfNotExists(
        this RouteHandlerBuilder routeHandlerBuilder,
        string groupName)
    {
        routeHandlerBuilder.Add(x =>
        {
            if (x.Metadata.All(w => w is not EndpointGroupNameAttribute))
            {
                x.Metadata.Add(
                    new EndpointGroupNameAttribute(groupName));
            }
        });

        return routeHandlerBuilder;
    }

    internal static RouteHandlerBuilder WithTagsIfNotExists(
        this RouteHandlerBuilder routeHandlerBuilder,
        params string[] tags)
    {
        routeHandlerBuilder.Add(x =>
        {
            if (x.Metadata.All(w => w is not TagsAttribute))
            {
                x.Metadata.Add(
                    new TagsAttribute(tags));
            }
        });

        return routeHandlerBuilder;
    }
}