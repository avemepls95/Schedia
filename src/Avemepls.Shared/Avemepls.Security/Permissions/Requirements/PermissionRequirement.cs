using Microsoft.AspNetCore.Authorization;

namespace Avemepls.Security.Permissions.Requirements;

internal sealed class PermissionRequirement(string permission) : IPermissionRequirement, IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}