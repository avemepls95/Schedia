using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Avemepls.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetDisplayName<T>(this T value)
        where T : struct, Enum
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null)
        {
            return null;
        }

        if (fieldInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true) is DisplayNameAttribute[] { Length: > 0 } displayNameAttributes)
        {
            return displayNameAttributes[0].DisplayName;
        }

        if (fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), true) is DisplayAttribute[] { Length: > 0 } displayAttributes)
        {
            return displayAttributes[0].Name;
        }

        return value.ToString();
    }
}