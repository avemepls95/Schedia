using Microsoft.AspNetCore.Authorization;

namespace Avemepls.Security.Permissions.Requirements;

internal sealed class RoleRequirement(string[] roles) : IRoleRequirement, IAuthorizationRequirement
{
    public string[] Roles { get; } = roles;
}