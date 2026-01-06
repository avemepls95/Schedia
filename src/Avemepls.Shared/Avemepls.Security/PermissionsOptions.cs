using System.Reflection;

namespace Avemepls.Security;

/// <summary>
/// Настройки подсистемы авторизации
/// </summary>
public class PermissionsOptions
{
    /// <summary>
    /// Признак неявного добавления прав
    /// </summary>
    public bool ImplicitRoles { get; private set; } = true;

    /// <summary>
    /// Сборки, по которым выполняется поиск прав
    /// </summary>
    public Assembly[] Assemblies { get; private set; }

    public PermissionsOptions AddAssemblies(params Assembly[] assemblies)
    {
        Assemblies = assemblies;
        return this;
    }

    public PermissionsOptions SetImplicitRoles(bool value)
    {
        ImplicitRoles = value;
        return this;
    }
}