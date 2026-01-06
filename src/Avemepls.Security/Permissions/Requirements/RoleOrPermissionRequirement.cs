using Microsoft.AspNetCore.Authorization;

namespace Avemepls.Security.Permissions.Requirements;

internal sealed class RoleOrPermissionRequirement(
    IRoleRequirement roleRequirement,
    IPermissionRequirement permissionRequirement)
    : IRoleRequirement, IPermissionRequirement, IAuthorizationRequirement
{
    public string[] Roles { get; } = roleRequirement.Roles;

    public string Permission { get; } = permissionRequirement.Permission;
}