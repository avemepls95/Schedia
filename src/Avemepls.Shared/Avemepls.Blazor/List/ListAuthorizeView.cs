using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Avemepls.Blazor.List;

/// <summary>
/// Displays different content depending on the user's authorization status.
/// </summary>
public class ListAuthorizeView : AuthorizeViewCore, IAuthorizeData
{
    /// <summary>
    /// Constructs an instance of <see cref="AuthorizeView"/>.
    /// </summary>
    public ListAuthorizeView()
    {
    }

    /// <summary>
    /// The policy name that determines whether the content can be displayed.
    /// </summary>
    [Parameter]
    public string? Policy { get; set; }

    /// <summary>
    /// A comma delimited list of roles that are allowed to display the content.
    /// </summary>
    [Parameter]
    public string? Roles { get; set; }

    public string? AuthenticationSchemes { get; set; }

    protected override IAuthorizeData[]? GetAuthorizeData()
    {
        if (string.IsNullOrEmpty(Policy) && string.IsNullOrEmpty(Roles))
            return null;
        return [this];
    }
}