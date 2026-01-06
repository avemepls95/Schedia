using System.Security.Principal;

namespace Avemepls.Security.Principal;

/// <summary>
/// This class is used to store (cache) principal during DI scope
/// </summary>
public class ScopePrincipalStorage
{
    /// <summary>
    /// Principal instance
    /// </summary>
    private IPrincipal? _principal;

    public IPrincipal? GetPrincipal() => _principal;

    public void SetPrincipal(IPrincipal? principal)
    {
        _principal = principal;
    }
}