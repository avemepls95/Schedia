namespace Avemepls.Security.Permissions;

public interface IPermissionsProvider
{
    Task<Permission[]> GetPermissions();
}