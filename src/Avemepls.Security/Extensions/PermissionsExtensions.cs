using System.Collections.ObjectModel;
using System.Reflection;

using Avemepls.Security.Attributes;

namespace Avemepls.Security.Extensions;

internal static class PermissionsExtensions
{
    internal static string ConcatAndSetPermission(this PropertyInfo property)
    {
        var type = property.DeclaringType;
        var chainOfNames = new List<string>();
        var iteratedType = type;

        while (!string.IsNullOrEmpty(iteratedType?.DeclaringType?.Name))
        {
            chainOfNames.Add(iteratedType.DeclaringType?.Name ?? string.Empty);
            iteratedType = iteratedType.DeclaringType;
        }

        chainOfNames.Reverse();
        chainOfNames.Add(type!.Name);
        chainOfNames.Add(property.Name);

        var propertyValue = string.Join('.', chainOfNames.Where(s => !string.IsNullOrEmpty(s)));
        property.SetValue(null, propertyValue);

        return propertyValue;
    }

    internal static IEnumerable<string> GetRelatedRoles(this IEnumerable<CustomAttributeData> attributes)
    {
        return attributes
            .Where(x => x.AttributeType == typeof(RequireRolesAttribute))
            .SelectMany(x => x.ConstructorArguments)
            .SelectMany(x =>
            {
                return x.Value is ReadOnlyCollection<CustomAttributeTypedArgument> arguments
                    ? arguments.Select(a => a.Value is string value ? value : string.Empty)
                    : [];
            });
    }
}