using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Avemepls.Core.Localization;

namespace Avemepls.Core.Extensions;

public static class DisplayNameLocExtensions
{
    public static string? GetEnumFieldDisplayName(this Enum? enumValue, string separator = ";")
    {
        if (enumValue == null)
        {
            return null;
        }

        var type = enumValue.GetType();

        if (type.IsDefined(typeof(FlagsAttribute), inherit: false))
        {
            return string.Join(separator, enumValue.GetFlagsEnumFieldDisplayName());
        }

        var memberInfo = type.GetMember(enumValue.ToString());
        return GetDisplayName(memberInfo);
    }

    private static string? GetDisplayName(MemberInfo[] memberInfo)
    {
        if (memberInfo.Length == 0)
        {
            return null;
        }

        var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute != null)
        {
            return displayAttribute.Name;
        }

        var displayNameLocAttribute = memberInfo[0].GetCustomAttribute<DisplayNameLocAttribute>();
        return displayNameLocAttribute?.DisplayName;
    }

    /// <summary>
    /// Получение всех значений, если enum с атрибутов Flags
    /// </summary>
    public static string[] GetFlagsEnumFieldDisplayName(this Enum value)
    {
        var enumType = value.GetType();
        var enumValues = Enum.GetValues(enumType).Cast<Enum>();
        var result = new Dictionary<int, string>();

        foreach (var enumValue in enumValues)
        {
            if (!value.HasFlag(enumValue))
            {
                continue;
            }

            var valueInt = Convert.ToInt32(enumValue);
            if (valueInt == 0 && result.Count > 0)
            {
                continue;
            }

            var displayName = GetDisplayName(enumType.GetMember(enumValue.ToString()));
            result[valueInt] = displayName;

            if (valueInt != 0 && result.ContainsKey(0))
            {
                result.Remove(0);
            }
        }

        return result.OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();
    }
}