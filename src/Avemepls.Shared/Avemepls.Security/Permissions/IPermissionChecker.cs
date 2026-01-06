namespace Avemepls.Security.Permissions;

/// <summary>
/// Проверяет доступ
/// </summary>
public interface IPermissionChecker
{
    Task<bool> CanUse(string permission);
}