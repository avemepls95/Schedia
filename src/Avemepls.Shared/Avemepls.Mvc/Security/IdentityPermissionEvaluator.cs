using System.Security.Claims;

using Avemepls.Domain.Security;
using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Authorization;

namespace Avemepls.Mvc.Security;

public abstract class IdentityPermissionEvaluator<TEntity>(
    IAuthorizationService authorizationService,
    IPrincipalAccessor principalAccessor)
    : IPermissionEvaluator<TEntity>
    where TEntity : class
{
    protected abstract string ReadPermission { get; }
    protected abstract string AddPermission { get; }
    protected abstract string EditPermission { get; }
    protected abstract string DeletePermission { get; }

    protected virtual async Task<bool> Authorize(TEntity entity, string permission, CancellationToken cancellationToken)
    {
        var principal = await principalAccessor.GetPrincipal();
        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            return (await authorizationService.AuthorizeAsync(claimsPrincipal, permission)).Succeeded;
        }

        return false;
    }

    public Task<bool> CanRead(TEntity entity, CancellationToken cancellationToken) => Authorize(entity, ReadPermission, cancellationToken);

    public Task<bool> CanAdd(TEntity entity, CancellationToken cancellationToken) => Authorize(entity, AddPermission, cancellationToken);

    public Task<bool> CanUpdate(TEntity entity, CancellationToken cancellationToken) => Authorize(entity, EditPermission, cancellationToken);

    public Task<bool> CanDelete(TEntity entity, CancellationToken cancellationToken) => Authorize(entity, DeletePermission, cancellationToken);
}