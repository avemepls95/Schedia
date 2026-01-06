using AntDesign;

using Microsoft.AspNetCore.Components;

namespace Schedia.Web.Core.Blazor.Helpers;

public static class ModalServiceHelpers
{
    // #PoIgnore#
    private static RenderFragment GetModalForCopy(string content) => builder =>
    {
        var index = 1;
#pragma warning disable ASP0006
        builder.OpenElement(index++, "div");
        builder.OpenElement(index++, "a");
        builder.AddContent(index++, content);
        builder.CloseElement();
        builder.OpenElement(index++, "hr");
        builder.CloseElement();
        builder.AddContent(index, "Скопируйте в буфер обмена");
        builder.CloseElement();
#pragma warning restore ASP0006
    };

    public static Task<bool> ShowCopyTextToClipboard(this ModalService modalService, string title, string absoluteUrl)
    {
        return modalService.InfoAsync(new ConfirmOptions
        {
            Title = title,
            Content = GetModalForCopy(absoluteUrl)
        });
    }
}