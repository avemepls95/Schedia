using System.IdentityModel.Tokens.Jwt;
using Avemepls.Auth.Bearer;
using Microsoft.AspNetCore.Components;

namespace Schedia.Web.MAUI.Services;

/// <summary>
/// MAUI implementation of IAuthenticationService.
/// </summary>
public class MauiAuthenticationService : Schedia.Web.Shared.Services.IAuthenticationService
{
    private readonly Schedia.Web.Shared.Services.IAuthStorageService _authStorageService;
    private readonly MauiAuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigationManager;

    public MauiAuthenticationService(
        Schedia.Web.Shared.Services.IAuthStorageService authStorageService,
        MauiAuthenticationStateProvider authStateProvider,
        NavigationManager navigationManager)
    {
        _authStorageService = authStorageService;
        _authStateProvider = authStateProvider;
        _navigationManager = navigationManager;
    }

    public async Task LoginAsync(TokenInformation tokenInformation, string? returnUrl = null)
    {
        // Store tokens in secure storage
        await _authStorageService.StoreTokensAsync(
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
        await _authStateProvider.LoginAsync(username, userId);

        // Navigate to return URL or home
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        _navigationManager.NavigateTo(redirectUrl, forceLoad: true);
    }

    public async Task LogoutAsync()
    {
        // Clear tokens from secure storage
        await _authStorageService.ClearTokensAsync();

        // Update authentication state
        await _authStateProvider.LogoutAsync();

        // Navigate to login
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }
}
