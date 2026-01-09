using System.Security.Claims;

namespace Schedia.Web.MAUI.Services;

public class MauiAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task LoginAsync(string username, string userId)
    {
        await SecureStorage.SetAsync("auth_user", username);
        await SecureStorage.SetAsync("auth_userId", userId);

        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "maui-auth");

        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove("auth_user");
        SecureStorage.Remove("auth_userId");

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }
}
