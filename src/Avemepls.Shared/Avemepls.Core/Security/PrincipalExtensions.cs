using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;

using Avemepls.Core.Extensions;

namespace Avemepls.Core.Security;

/// <summary>
/// Extensions for the <see cref="IPrincipal"/>.
/// </summary>
public static class PrincipalExtensions
{
    /// <summary>
    /// Returns first claim value of specified type
    /// </summary>
    /// <param name="principal">Principal instance, should be of type <see cref="ClaimsPrincipal"/></param>
    /// <param name="claimType">Type of claim to return</param>
    public static string? GetClaimValue(this IPrincipal principal, string claimType) => principal.GetClaim(claimType)?.Value;

    /// <summary>
    /// Ensures that user has all specified permissions
    /// </summary>
    /// <param name="principal">Principal instance, should be of type <see cref="ClaimsPrincipal"/></param>
    /// <param name="permissions">List of permissions to check</param>
    public static bool HasPermissions(this IPrincipal principal, params string[] permissions)
    {
        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            return permissions.TrueForAll(p => claimsPrincipal.HasClaim(Constants.ClaimTypes.Permission, p));
        }

        return false;
    }

    /// <summary>
    /// Ensures that user has all specified roles
    /// </summary>
    /// <param name="principal">Principal instance, should be of type <see cref="ClaimsPrincipal"/></param>
    /// <param name="roles">List of roles to check</param>
    public static bool IsInRoles(this IPrincipal principal, params string[] roles)
    {
        if (roles.Length == 0)
        {
            return true;
        }

        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            return roles.Exists(r => claimsPrincipal.IsInRole(r));
        }

        return false;
    }

    /// <summary>
    /// Returns first claim of specified type
    /// </summary>
    /// <param name="principal">Principal instance, should be of type <see cref="ClaimsPrincipal"/></param>
    /// <param name="claimType">Type of claim to return</param>
    public static Claim? GetClaim(this IPrincipal principal, string claimType)
    {
        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.FirstOrDefault(x => x.Type == claimType);
        }

        return null;
    }

    /// <summary>
    /// Returns value of claim by the specified type
    /// </summary>
    public static bool TryGetClaimValue<TValue>(
        this IPrincipal principal,
        string claimType,
        [MaybeNullWhen(false)]
        out TValue value)
        where TValue : IConvertible
    {
        var claim = principal.GetClaim(claimType);

        if (string.IsNullOrWhiteSpace(claim?.Value))
        {
            value = default;

            return false;
        }

        try
        {
            value = (TValue)Convert.ChangeType(claim.Value, typeof(TValue));

            return true;
        }
        catch
        {
            value = default;

            return false;
        }
    }

    public static string? FindFirstValue(this ClaimsPrincipal principal, string claimName)
    {
        return principal.FindFirst(claimName)?.Value;
    }

    /// <summary>
    /// Returns user's email
    /// </summary>
    /// <param name="principal">Principal.</param>
    public static string? GetEmail(this IPrincipal principal)
    {
        var claimsPrincipal = principal as ClaimsPrincipal;

        return claimsPrincipal?.FindFirstValue("email") ?? claimsPrincipal?.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Returns user's phone
    /// </summary>
    public static string? GetPhone(this IPrincipal principal)
    {
        var claimsPrincipal = principal as ClaimsPrincipal;

        return claimsPrincipal?.FindFirstValue("phone_number") ??
               claimsPrincipal?.FindFirstValue(ClaimTypes.MobilePhone);
    }

    /// <summary>
    /// Checks if user has id
    /// </summary>
    public static bool HasSubjectId(this IPrincipal principal)
    {
        var id = principal.Identity as ClaimsIdentity;
        var claim = id?.FindFirst("sub");

        return claim != null && !string.IsNullOrEmpty(claim.Value);
    }

    /// <summary>
    /// Get user id
    /// </summary>
    public static int? GetId(this IPrincipal principal)
    {
        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            if (int.TryParse(claimsPrincipal.FindFirstValue(Constants.ClaimTypes.UserId), out var userId))
                return userId;

            return null;
        }

        return null;
    }

    /// <summary>
    /// Check if user has roles
    /// </summary>
    public static bool IsInRole(this IPrincipal principal, params string[] roles)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return roles?.Aggregate(false, (result, role) => result || principal.IsInRole(role))
               ?? throw new ArgumentNullException(nameof(roles));
    }
}