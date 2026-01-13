using System.IdentityModel.Tokens.Jwt;
using Avemepls.Auth.Bearer;
using Microsoft.AspNetCore.Components;

namespace Schedia.Web.MAUI.Services;

/// <summary>
/// MAUI implementation of IAuthenticationService.
/// </summary>
public class MauiAuthenticationService(
    Schedia.Web.Shared.Services.IAuthStorageService authStorageService,
    MauiAuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager)
    : Schedia.Web.Shared.Services.IAuthenticationService
{
    public async Task LoginAsync(TokenInformation tokenInformation, string? returnUrl = null)
    {
        // Store tokens in secure storage
        await authStorageService.StoreTokensAsync(
            tokenInformation.AccessToken,
            tokenInformation.RefreshToken,
            tokenInformation.ExpiresIn);

        // Parse JWT token to extract user info
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenInformation.AccessToken);

        // Get username and user ID from claims
        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name");
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid");

        var username = usernameClaim?.Value ?? "Unknown";
        var userId = userIdClaim?.Value ?? string.Empty;

        // Update authentication state
        await authStateProvider.LoginAsync(username, userId);

        // Navigate to return URL or home
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        navigationManager.NavigateTo(redirectUrl, forceLoad: true);
    }

    public async Task LogoutAsync()
    {
        // Clear tokens from secure storage
        await authStorageService.ClearTokensAsync();

        // Update authentication state
        await authStateProvider.LogoutAsync();

        // Navigate to login
        navigationManager.NavigateTo("/login", forceLoad: true);
    }
}
