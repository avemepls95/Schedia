namespace Avemepls.Security.Attributes;

/// <summary>
/// Лицензированные доступы
/// Если указан аттрибут, лицензия будет требовать наличие этих доступов при использовании
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class LicensePermissionAttribute : Attribute
{
}