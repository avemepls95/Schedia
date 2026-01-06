using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Avemepls.Security.Attributes;
using Avemepls.Security.Extensions;

namespace Avemepls.Security.Permissions;

/// <summary>
/// Ищет в сборках все типы декорированные атрибутом PermissionsData и формирует из них массив типа
/// "Имя подсистемы - Имя вложенного типа - Массив прав этого типа"
/// </summary>
public class AssemblyScanPermissionsProvider : IPermissionsProvider
{
    private readonly Assembly[] _assemblies;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="assemblies">Сборки в которых будет происходит поиск</param>
    public AssemblyScanPermissionsProvider(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public Permission[] GetPermissionsFromAssemblies()
    {
        var result = new List<Permission>();

        foreach (var asm in _assemblies)
        {
            foreach (var moduleType in asm.GetTypes())
            {
                var permissionDataAttribute = moduleType.GetCustomAttribute<PermissionsDataAttribute>();

                if (permissionDataAttribute == null)
                    continue;

                var moduleName = permissionDataAttribute.ModuleName;

                var moduleRoles = moduleType.CustomAttributes.GetRelatedRoles().ToArray();

                foreach (var property in moduleType.GetProperties())
                {
                    var entityDescription = GetDescription(property);
                    var propertyRoles = property.CustomAttributes.GetRelatedRoles().ToArray();
                    var permissionName = property.ConcatAndSetPermission();

                    if (!string.IsNullOrEmpty(permissionName))
                    {
                        var permission = Permission.Parse(permissionName,
                                                          moduleRoles.Union(propertyRoles).ToArray(),
                                                          $"{moduleName}.{entityDescription}");

                        result.Add(permission);
                    }
                }

                var moduleConstants = moduleType.GetFields(BindingFlags.Public
                                                           | BindingFlags.Static
                                                           | BindingFlags.FlattenHierarchy);

                foreach (var constant in moduleConstants.Where(constant => constant is
                                                                   { IsLiteral: true, IsInitOnly: false }))
                {
                    var constantDescription = GetDescription(constant);
                    var constantRoles = constant.CustomAttributes.GetRelatedRoles().ToArray();

                    var permissionValue = constant.GetValue(null)?.ToString();

                    if (!string.IsNullOrEmpty(permissionValue))
                    {
                        var permission = Permission.Parse(permissionValue,
                                                          moduleRoles.Union(constantRoles).ToArray(),
                                                          $"{moduleName}.{constantDescription}");

                        result.Add(permission);
                    }
                }

                // Nested type
                foreach (var entityType in moduleType.GetNestedTypes())
                {
                    var entityRoles = moduleRoles
                        .Union(entityType.CustomAttributes.GetRelatedRoles())
                        .Distinct()
                        .ToArray();

                    var entityDescription = GetDescription(entityType);

                    foreach (var property in entityType.GetProperties())
                    {
                        var actionDescription = GetDescription(property);
                        var propertyRoles = property.CustomAttributes.GetRelatedRoles().ToArray();

                        var permissionName = property.ConcatAndSetPermission();

                        if (!string.IsNullOrEmpty(permissionName))
                        {
                            var permission = Permission.Parse(permissionName,
                                                              entityRoles.Union(propertyRoles).ToArray(),
                                                              $"{moduleName}.{entityDescription}.{actionDescription}");

                            result.Add(permission);
                        }
                    }

                    var constants = entityType.GetFields(BindingFlags.Public
                                                         | BindingFlags.Static
                                                         | BindingFlags.FlattenHierarchy);

                    foreach (var constant in constants.Where(fi => fi.IsLiteral && !fi.IsInitOnly))
                    {
                        var actionDescription = GetDescription(constant);
                        var constantRoles = constant.CustomAttributes.GetRelatedRoles().ToArray();

                        var permissionValue = constant.GetValue(null)?.ToString();

                        if (!string.IsNullOrEmpty(permissionValue))
                        {
                            var permission = Permission.Parse(permissionValue,
                                                              entityRoles.Union(constantRoles).ToArray(),
                                                              $"{moduleName}.{entityDescription}.{actionDescription}");

                            result.Add(permission);
                        }
                    }
                }
            }
        }

        return result.ToArray();
    }

    public Task<Permission[]> GetPermissions()
    {
        return Task.FromResult(GetPermissionsFromAssemblies());
    }

    private static string GetDescription(PropertyInfo nestedType)
    {
        return nestedType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
               ?? nestedType.GetCustomAttribute<DisplayAttribute>()?.Name
               ?? nestedType.Name;
    }

    private static string GetDescription(FieldInfo nestedType)
    {
        return nestedType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
               ?? nestedType.GetCustomAttribute<DisplayAttribute>()?.Name
               ?? nestedType.Name;
    }

    private static string GetDescription(Type nestedType)
    {
        return nestedType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
               ?? nestedType.GetCustomAttribute<DisplayAttribute>()?.Name
               ?? nestedType.Name;
    }
}