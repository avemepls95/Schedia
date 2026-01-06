using System.Security.Principal;

namespace Avemepls.Infrastructure.Identity;

internal sealed class AnonymousIdentity : IIdentity
{
    public string? AuthenticationType => "None";

    public bool IsAuthenticated => false;

    public string? Name { get; } = Guid.NewGuid().ToString();
}