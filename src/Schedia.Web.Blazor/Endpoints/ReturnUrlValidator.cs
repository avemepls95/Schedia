namespace Schedia.Web.Blazor.Endpoints;

public static class ReturnUrlValidator
{
    public static string GetSafeReturnUrl(string? returnUrl, string fallback = "/")
    {
        return IsLocalUrl(returnUrl) ? returnUrl! : fallback;
    }

    public static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (url[0] == '/')
        {
            return url.Length == 1 || (url[1] != '/' && url[1] != '\\');
        }

        if (url.Length > 1 && url[0] == '~' && url[1] == '/')
        {
            return url.Length == 2 || (url[2] != '/' && url[2] != '\\');
        }

        return false;
    }
}