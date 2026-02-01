using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Avemepls.Auth.Bearer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Schedia.Web.Blazor.Services;

/// <summary>
/// Web implementation of IAuthenticationService using Cookie authentication only.
/// </summary>
public class WebAuthenticationService(
    TokenAuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime)
    : Schedia.Web.Base.Services.IAuthenticationService
{
    public async Task LoginAsync(TokenInformation tokenInformation, string? returnUrl = null)
    {
        // Set cookie via API endpoint (JS interop needed for browser to receive Set-Cookie header)
        var success = await jsRuntime.InvokeAsync<bool>(
            "tools.auth.setAuthCookie",
            tokenInformation.AccessToken,
            tokenInformation.ExpiresIn);

        if (!success)
        {
            throw new InvalidOperationException("Failed to set authentication cookie");
        }

        // Parse JWT token to extract claims for immediate UI update
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenInformation.AccessToken);

        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        // Notify authentication state changed for immediate UI update
        authStateProvider.NotifyUserAuthentication(user);

        // Navigate to return URL or home
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        navigationManager.NavigateTo(redirectUrl, forceLoad: false);
    }

    public async Task LogoutAsync()
    {
        // Clear cookie via API endpoint
        await jsRuntime.InvokeAsync<bool>("tools.auth.clearAuthCookie");

        // Notify authentication state changed
        authStateProvider.NotifyUserLogout();

        // Navigate to login
        navigationManager.NavigateTo("/login", forceLoad: false);
    }
}