namespace Schedia.Web.Base.Services;

/// <summary>
/// Service for storing and retrieving authentication tokens.
/// </summary>
public interface IAuthStorageService
{
    /// <summary>
    /// Stores authentication tokens.
    /// </summary>
    Task StoreTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiresIn);

    /// <summary>
    /// Retrieves the access token.
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Retrieves the refresh token.
    /// </summary>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>
    /// Retrieves the token expiration time.
    /// </summary>
    Task<DateTimeOffset?> GetExpiresInAsync();

    /// <summary>
    /// Clears all stored tokens.
    /// </summary>
    Task ClearTokensAsync();
}
