using Avemepls.Auth.Bearer;

namespace Schedia.Web.Base.Services;

/// <summary>
/// Service for handling authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Performs login with the provided token information.
    /// </summary>
    Task LoginAsync(TokenInformation tokenInformation, string? returnUrl = null);

    /// <summary>
    /// Performs logout.
    /// </summary>
    Task LogoutAsync();
}