using System.Globalization;

using Microsoft.Extensions.Localization;

namespace Avemepls.Blazor.Components;

public static class Extensions
{
    /// <summary>
    /// Возвращает локализованную строку
    /// Если текущий язык не русский и перевод не был найден, то возвращает исходную строку в квадратных скобках
    /// </summary>
    public static string Loc(this string value, IStringLocalizer? loc, params object[] args)
    {
        if (loc is null || string.IsNullOrEmpty(value))
            return value;

        var locTitle = loc[value, args];
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "ru" && locTitle == value ? $"[{locTitle}]" : locTitle;
    }
}