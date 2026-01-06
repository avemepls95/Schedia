using Microsoft.AspNetCore.WebUtilities;

namespace Avemepls.Mvc;

/// <summary>
/// Extensions for the <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Получить тип ссылки.
    /// </summary>
    public static UriKind GetUrlKind(this string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        return url.StartsWith(Uri.UriSchemeFile) ||
               url.StartsWith(Uri.UriSchemeFtp) ||
               url.StartsWith(Uri.UriSchemeHttp) ||
               url.StartsWith(Uri.UriSchemeHttps) ||
               url.StartsWith(Uri.UriSchemeGopher) ||
               url.StartsWith(Uri.UriSchemeMailto) ||
               url.StartsWith(Uri.UriSchemeNews) ||
               url.StartsWith(Uri.UriSchemeNntp) ||
               url.StartsWith(Uri.UriSchemeNetPipe) ||
               url.StartsWith(Uri.UriSchemeNetTcp)
            ? UriKind.Absolute
            : UriKind.Relative;
    }

    /// <summary>
    /// Удалить из URL query параметры.
    /// </summary>
    public static string RemoveUrlQuery(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        var kind = url.GetUrlKind();

        if (kind == UriKind.Absolute)
        {
            return new Uri(url).GetComponents(
                UriComponents.AbsoluteUri & ~UriComponents.Query,
                UriFormat.Unescaped);
        }

        var questionIndex = url.IndexOf('?');
        return questionIndex > -1
            ? url.Substring(0, questionIndex)
            : url;
    }

    /// <summary>
    /// Добавить параметр в query.
    /// </summary>
    public static string AddQueryParam(this string url, string name, string value) =>
        QueryHelpers.AddQueryString(url, name, value);
}