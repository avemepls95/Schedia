using System.Security.Principal;

using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace Schedia.Web.Core.Blazor;

public class BlazorPrincipalAccessor(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authStateProvider,
    ScopePrincipalStorage scopePrincipalStorage)
    : IPrincipalAccessor
{
    public async Task<IPrincipal> GetPrincipal()
    {
        var principal = scopePrincipalStorage.GetPrincipal();

        if (principal?.Identity is not null && principal.Identity.IsAuthenticated)
        {
            return principal;
        }

        var authState = await authStateProvider.GetAuthenticationStateAsync();

        principal = (authState.User.Identity?.IsAuthenticated == true
                        ? authState.User
                        : httpContextAccessor.HttpContext?.User) ??
                    IPrincipalAccessor.PrincipalUnauthorized;

        scopePrincipalStorage.SetPrincipal(principal);

        return principal;
    }
}