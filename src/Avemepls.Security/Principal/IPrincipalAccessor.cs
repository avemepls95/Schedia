using System.Security.Claims;
using System.Security.Principal;

namespace Avemepls.Security.Principal;

/// <summary>
/// Factory to access current principal
/// </summary>
public interface IPrincipalAccessor
{
    /// <summary>
    /// Returns current principal
    /// </summary>
    Task<IPrincipal> GetPrincipal();

    /// <summary>
    /// Returns unauthorized principal
    /// </summary>
    public static readonly IPrincipal PrincipalUnauthorized = new ClaimsPrincipal();
}