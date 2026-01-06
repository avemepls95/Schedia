using System.Text;

using Microsoft.AspNetCore.Components;

namespace Avemepls.Blazor.Components;

public static class RenderFragmentsRenderer
{
    private static RenderFragment CreateDayCell(DateTime date, string style, string classes) => builder =>
    {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", $"ant-picker-cell-inner {classes}");
        builder.AddAttribute(3, "style", style);
        builder.AddContent(4, date.Day);
        builder.CloseElement();
    };

    public static Func<DateTime, DateTime, RenderFragment> GetDateFragment(Func<DateTime, DateTime, string>? dateRenderStyle)
        => (date, today) =>
        {
            var classes = new StringBuilder();
            var style = new StringBuilder("color: black; ");

            if (dateRenderStyle is not null)
            {
                var overrideStyle = dateRenderStyle.Invoke(date, today);

                style.Append(' ');
                style.Append(overrideStyle);
            }

            return CreateDayCell(date, style.ToString(), classes.ToString());
        };
}