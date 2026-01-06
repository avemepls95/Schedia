using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Avemepls.Mvc;

/// <summary>
/// <see cref="IUrlHelper"/> extension methods.
/// </summary>
public static class UrlHelperExtensions
{
    /// <summary>
    /// Generates a fully qualified URL to an action method by using the specified action name, controller name and
    /// route values.
    /// </summary>
    /// <param name="url">The URL helper.</param>
    /// <param name="actionName">The name of the action method.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The route values.</param>
    /// <returns>The absolute URL.</returns>
    public static string AbsoluteAction(
        this IUrlHelper url,
        string actionName,
        string controllerName,
        object? routeValues = null)
    {
        return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme)!;
    }

    /// <summary>
    /// Generates a fully qualified URL to the specified content by using the specified content path. Converts a
    /// virtual (relative) path to an application absolute path.
    /// </summary>
    /// <param name="url">The URL helper.</param>
    /// <param name="contentPath">The content path.</param>
    /// <returns>The absolute URL.</returns>
    public static string AbsoluteContent(
        this IUrlHelper url,
        string contentPath)
    {
        var request = url.ActionContext.HttpContext.Request;
        return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
    }

    /// <summary>
    /// Generates a fully qualified URL to the specified route by using the route name and route values.
    /// </summary>
    /// <param name="url">The URL helper.</param>
    /// <param name="routeName">Name of the route.</param>
    /// <param name="routeValues">The route values.</param>
    /// <returns>The absolute URL.</returns>
    public static string AbsoluteRouteUrl(
        this IUrlHelper url,
        string routeName,
        object? routeValues = null)
    {
        return url.RouteUrl(routeName, routeValues, url.ActionContext.HttpContext.Request.Scheme)!;
    }

    public static string GenerateConfirmEmailChange(this IUrlHelper url, string userId, string email, string code)
    {
        ArgumentNullException.ThrowIfNull(url);

        var result = url.AbsoluteAction("ConfirmEmailChange", "Account");
        result = QueryHelpers.AddQueryString(result, "userId", userId);
        result = QueryHelpers.AddQueryString(result, "email", email);
        result = QueryHelpers.AddQueryString(result, "code", code);
        return result;
    }

    public static string GenerateConfirmEmail(this IUrlHelper url, string code, string userId)
    {
        ArgumentNullException.ThrowIfNull(url);

        var result = url.AbsoluteAction("ConfirmEmail", "Account");
        result = QueryHelpers.AddQueryString(result, "code", code);
        result = QueryHelpers.AddQueryString(result, "userId", userId);
        return result;
    }
}