using System.Security.Principal;

namespace Schedia.Web.MAUI.Services;

public class MauiPrincipalAccessor(AuthenticationStateProvider authStateProvider) : IPrincipalAccessor
{
    public async Task<IPrincipal> GetPrincipal()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        return authState.User;
    }
}
