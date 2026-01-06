namespace Avemepls.Security.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PermissionsDataAttribute(string moduleName) : Attribute
{
    public string ModuleName { get; set; } = moduleName;
}