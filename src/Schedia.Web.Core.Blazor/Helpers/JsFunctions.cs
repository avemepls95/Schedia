using Microsoft.JSInterop;

namespace Schedia.Web.Core.Blazor.Helpers;

public static class JsFunctions
{
    /// <summary>
    /// Загрузка файла пользователю
    /// </summary>
    public static async ValueTask DownloadFile(this IJSRuntime jsRuntime, string fileName, Stream content)
    {
        using var streamRef = new DotNetStreamReference(content);

        await jsRuntime.InvokeVoidAsync("tools.interop.downloadFileFromStream", fileName, streamRef);
    }
}