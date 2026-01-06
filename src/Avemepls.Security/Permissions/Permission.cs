namespace Avemepls.Security.Permissions;

/// <summary>
/// Доступ
/// </summary>
public sealed record Permission
{
    private const char Separator = '.';

    /// <summary>
    /// Полное наименование-ключ
    /// </summary>
    public string FullName
    {
        get
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(ModuleName))
            {
                value += ModuleName;
            }

            if (!string.IsNullOrEmpty(EntityName))
            {
                value += Separator + EntityName;
            }

            if (!string.IsNullOrEmpty(ActionName))
            {
                value += Separator + ActionName;
            }

            return value;
        }
    }

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Roles in which this permission is included
    /// </summary>
    public string[] Roles { get; }

    /// <summary>
    /// Наименование модуля
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Наименование сущности
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Наименование действия
    /// </summary>
    public string? ActionName { get; }

    /// <summary>
    /// Тип
    /// </summary>
    public Types Type => string.IsNullOrEmpty(ActionName)
        ? Types.Resource
        : Types.Action;

    /// <summary>
    /// Constructor
    /// </summary>
    private Permission(
        string moduleName,
        string? entityName = null,
        string? actionName = null,
        string[]? roles = null,
        string? description = null)
    {
        ValidateLastPart(entityName);
        ValidateLastPart(actionName);

        ModuleName = moduleName;
        EntityName = entityName;
        ActionName = actionName;
        Roles = roles ?? [];
        Description = description;
    }

    public override string ToString() => FullName;

    /// <summary>
    /// Вид доступа
    /// </summary>
    public enum Types
    {
        /// <summary>
        /// Ресурс
        /// </summary>
        Resource,

        /// <summary>
        /// Действие к ресурсу
        /// </summary>
        Action
    }

    public static Permission CreateResource(
        string resourceName,
        string[]? roles = null,
        string? description = null)
    {
        return new Permission(resourceName, roles: roles, description: description);
    }

    public static Permission CreateEntity(
        string moduleName,
        string entityName,
        string[]? roles = null,
        string? description = null,
        bool isLicensed = false)
    {
        return new Permission(moduleName,
                              entityName: entityName,
                              roles: roles,
                              description: description);
    }

    public static Permission CreateAction(
        string moduleName,
        string entityName,
        string actionName,
        string[]? roles = null,
        string? description = null,
        bool isLicensed = false)
    {
        return new Permission(moduleName,
                              entityName,
                              actionName,
                              roles,
                              description);
    }

    private static void ValidateLastPart(string? value)
    {
        if (!string.IsNullOrEmpty(value) && value.Contains(Separator))
        {
            throw new ArgumentException($"Last part of permission can't contain '{Separator}' symbol");
        }
    }

    /// <summary>
    /// Парсит строку в доступ
    /// </summary>
    public static Permission Parse(string value, string[]? roles = null, string? description = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        var parts = value.Split(Separator);

        string moduleName;
        string? entityName = null;
        string? actionName = null;

        switch (parts.Length)
        {
            case >= 3:
                moduleName = string.Join(Separator, parts.Take(parts.Length - 2));
                entityName = parts[^2];
                actionName = parts[^1];

                break;

            case 2:
                moduleName = parts[0];
                entityName = parts[^1];

                break;

            default:
                moduleName = parts[0];

                break;
        }

        return new Permission(moduleName, entityName, actionName, roles, description);
    }
}