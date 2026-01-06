using System.Globalization;

using Microsoft.Extensions.Localization;

namespace Avemepls.Core.Localization;

/// <summary>
/// Контекст локаизации
/// </summary>
public interface ILocalizationContext
{
    IStringLocalizerFactory? GetLocalizerFactory();

    public static ILocalizationContext? Instance { get; set; }

    public static string GetString<TResourceType>(string value)
    {
        var loc = Instance?.GetLocalizerFactory()?.Create(typeof(TResourceType));

        var locTitle = loc is null
            ? value
            : loc[value];

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "ru" && locTitle == value
            ? $"[{locTitle}]"
            : locTitle;
    }

    public static string GetString(string value, Type? rsourceType = null)
    {
        var loc = Instance?.GetLocalizerFactory()?.Create(rsourceType ?? typeof(string));

        var locTitle = loc is null
            ? value
            : loc[value];

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "ru" && locTitle == value
            ? $"[{locTitle}]"
            : locTitle;
    }
}