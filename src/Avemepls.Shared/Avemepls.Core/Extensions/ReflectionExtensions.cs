using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Avemepls.Core.Extensions;

public static class ReflectionExtensions
{
    /// <summary>
    /// Возвращает название из атрибута <see cref="DisplayNameAttribute"/> для свойства
    /// </summary>
    public static string GetDisplayName(this PropertyInfo propertyInfo)
    {
        var attr = propertyInfo.GetCustomAttributes()
            .FirstOrDefault(ca => ca is DisplayNameAttribute);
        return (attr != null ? (attr as DisplayNameAttribute)?.DisplayName : propertyInfo.Name) ?? propertyInfo.Name;
    }

    /// <summary>
    /// Возвращает формат из атрибута <see cref="DisplayNameAttribute"/> для свойства
    /// </summary>
    public static string GetDisplayFormat(this PropertyInfo propertyInfo)
    {
        var attr = propertyInfo.GetCustomAttributes()
            .FirstOrDefault(ca => ca is DisplayFormatAttribute);
        return (attr != null ? (attr as DisplayFormatAttribute)?.DataFormatString : string.Empty) ?? string.Empty;
    }

    /// <summary>
    /// Идентификатор типа для документации API
    /// </summary>
    /// <param name="processedType">Тип, для которого создается идентификатор</param>
    /// <returns>идентификатор</returns>
    public static string GetOpenApiName(this Type processedType)
    {
        string GetTypeName(Type type)
        {
            var declaringTypeName = string.Empty;
            if (type.DeclaringType != null)
                declaringTypeName = type.DeclaringType.Name + ".";

            if (type.IsGenericType)
            {
                var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
                var genericTypeNames = type.GenericTypeArguments.Select(GetTypeName);
                var genericArgumentsTypesPart = string.Join(",", genericTypeNames);
                return $"{genericTypeName}<{genericArgumentsTypesPart}>";
            }

            return $"{declaringTypeName}{type.Name}";
        }

        var typeName = GetTypeName(processedType);
        return typeName;
    }
}