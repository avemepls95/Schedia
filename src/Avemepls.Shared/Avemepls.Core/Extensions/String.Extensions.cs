using System.Globalization;
using System.Text.Json;

namespace Avemepls.Core.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string source)
    {
        return string.Concat(source.Select((x, i) => i > 0 && char.IsUpper(x)
                                               ? "_" + x
                                               : x.ToString()))
            .ToLowerInvariant();
    }

    public static string ToPascalCaseFromSnakeCase(this string str) =>
        string.Concat(
            str.Split('_')
                .Select(CultureInfo.CurrentCulture.TextInfo.ToTitleCase));

    public static bool IsJson(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using (JsonDocument.Parse(json))
            {
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }
}