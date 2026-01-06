using Avemepls.Security.Attributes;

using Microsoft.Extensions.Caching.Memory;

namespace Avemepls.Security.Permissions;

/// <summary>
/// Хранилище пермишенов приложения. Заполняется путем сканирования всех сборок приложения
/// И поиска в них типов, декорированных атрибутом <see cref="PermissionsDataAttribute"/>.
/// </summary>
public class PermissionRegistry
{
    private readonly IEnumerable<IPermissionsProvider> _providers;
    private readonly IMemoryCache _memoryCache;

    private const string CacheKey = "system:permissions";

    /// <summary>
    /// Initializes new instance
    /// </summary>
    public PermissionRegistry(
        IEnumerable<IPermissionsProvider> providers,
        IMemoryCache memoryCache)
    {
        _providers = providers;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Returns full list of permissions
    /// </summary>
    public async Task<Permission[]> GetAll()
    {
        if (_memoryCache.TryGetValue(CacheKey, out Permission[]? permissions) && permissions is not null)
        {
            return permissions;
        }

        var initPermissions = _providers.Select(async p => await p.GetPermissions()).ToArray();

        await Task.WhenAll(initPermissions);

        permissions = initPermissions.SelectMany(x => x.Result).ToArray();

        _memoryCache.Set(CacheKey, permissions);

        return permissions;
    }
}