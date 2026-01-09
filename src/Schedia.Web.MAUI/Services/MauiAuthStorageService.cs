using Schedia.Web.Shared.Services;

namespace Schedia.Web.MAUI.Services;

/// <summary>
/// MAUI implementation of IAuthStorageService using SecureStorage.
/// </summary>
public class MauiAuthStorageService : IAuthStorageService
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string ExpiresInKey = "auth_expires_in";

    public async Task StoreTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiresIn)
    {
        await SecureStorage.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.SetAsync(ExpiresInKey, expiresIn.ToString("O"));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.GetAsync(AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.GetAsync(RefreshTokenKey);
    }

    public async Task<DateTimeOffset?> GetExpiresInAsync()
    {
        var value = await SecureStorage.GetAsync(ExpiresInKey);
        return DateTimeOffset.TryParse(value, out var result) ? result : null;
    }

    public Task ClearTokensAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(ExpiresInKey);
        return Task.CompletedTask;
    }
}
