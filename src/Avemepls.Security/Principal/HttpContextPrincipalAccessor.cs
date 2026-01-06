using System.Security.Principal;

using Microsoft.AspNetCore.Http;

namespace Avemepls.Security.Principal;

/// <summary>
/// Factory to access current principal from current http context
/// </summary>
internal sealed class HttpContextPrincipalAccessor(IHttpContextAccessor httpContextAccessor) : IPrincipalAccessor
{
    public Task<IPrincipal> GetPrincipal()
    {
        return httpContextAccessor.HttpContext == null
            ? Task.FromResult(IPrincipalAccessor.PrincipalUnauthorized)
            : Task.FromResult<IPrincipal>(httpContextAccessor.HttpContext.User);
    }
}