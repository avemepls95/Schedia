namespace Avemepls.Security.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class RequireRolesAttribute : Attribute
{
    public RequireRolesAttribute(params string[] roles)
    {
    }
}