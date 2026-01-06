using Avemepls.Core.Security;
using Avemepls.Security.Permissions.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace Avemepls.Security.Permissions;

internal sealed class PermissionAuthorizationHandler(IPermissionChecker? licensedPermissionChecker = null) : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is RoleOrPermissionRequirement roleOrPermissionRequirement)
            {
                if (IsRoleRequirementSatisfied(context, roleOrPermissionRequirement)
                    || await IsPermissionRequirementSatisfied(context, roleOrPermissionRequirement))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else if (requirement is IRoleRequirement roleRequirement)
            {
                if (IsRoleRequirementSatisfied(context, roleRequirement))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else if (requirement is IPermissionRequirement permissionRequirement)
            {
                if (await IsPermissionRequirementSatisfied(context, permissionRequirement))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
        }
    }

    private static bool IsRoleRequirementSatisfied(AuthorizationHandlerContext context, IRoleRequirement roleRequirement)
    {
        return context.User.IsInRoles(roleRequirement.Roles);
    }

    private async Task<bool> IsPermissionRequirementSatisfied(
        AuthorizationHandlerContext context,
        IPermissionRequirement permissionRequirement)
    {
        if (!context.User.HasPermissions(permissionRequirement.Permission))
        {
            return false;
        }

        return licensedPermissionChecker is null || await licensedPermissionChecker.CanUse(permissionRequirement.Permission);
    }
}