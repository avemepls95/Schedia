using AntDesign;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Avemepls.Blazor.Components;

public class ExtendedDatePicker<TValue> : DatePicker<TValue>
{
    [Inject]
    private IServiceProvider ServiceProvider { get; set; }

    [Parameter]
    public Func<DateTime, DateTime, string>? DateRenderStyle { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenElement(1, "div");
        __builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, DatePickerClick));

        base.BuildRenderTree(__builder);

        __builder.CloseElement();
    }

    public async Task DatePickerClick()
    {
        if (!_needRefresh)
        {
            await _inputStart.OnClick.InvokeAsync();
        }
    }
}