using AntDesign;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Avemepls.Blazor.Components;

public class EditableColumn<TData> : Column<TData>
{
    [Parameter]
    public string? EditingId { get; set; }

    [Parameter]
    public EventCallback<string?> EditingIdChanged { get; set; }

    [Parameter]
    public EventCallback OnBlur { get; set; }

    [Parameter]
    public EventCallback<string> OnStartEditing { get; set; }

    [Parameter]
    public Func<Task<bool>>? CanEdit { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string InputType { get; set; }

    [Parameter]
    public object? MaxValue { get; set; }

    [Parameter]
    public object? MinValue { get; set; }

    [Parameter]
    public string ColumnId { get; set; } = Guid.NewGuid().ToString();

    [Parameter]
    public int? Precision { get; set; }

    [Parameter]
    public string? Prefix { get; set; }

    /// <summary>
    /// Formatter from number to string for displaying
    /// </summary>
    [Parameter]
    public Func<TData, string?> Formatter { get; set; } = value => value?.ToString();

    private async Task SetEditingId(string? value)
    {
        EditingId = value;
        await EditingIdChanged.InvokeAsync(value);
    }

    private async Task OnChange(TData value)
    {
        await FieldChanged.InvokeAsync(value);
    }

    private async Task OnBlured()
    {
        if (OnBlur.HasDelegate) await OnBlur.InvokeAsync();
        await SetEditingId(null);
        StateHasChanged();
    }

    private async Task OnCellClick()
    {
        if (OnStartEditing.HasDelegate) await OnStartEditing.InvokeAsync();

        if (CanEdit != null)
        {
            var canEdit = await CanEdit!.Invoke();

            if (!canEdit) return;
        }

        await SetEditingId(ColumnId);
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (string.IsNullOrWhiteSpace(InputType))
        {
            InputType = typeof(TData) == typeof(string)
                ? "text"
                : "number";
        }

        if (typeof(TData) == typeof(decimal) || typeof(TData) == typeof(decimal?))
        {
            Formatter = value => FormatDecimal((decimal?)((object?)value));
        }

        ChildContent = builder =>
        {
            if (EditingId == ColumnId)
            {
                if (InputType == "number")
                {
                    builder.OpenComponent<InputNumber<TData>>(0);
                    builder.AddAttribute(1, "TValue", typeof(TData));
                    builder.AddAttribute(2, "Value", Field);
                    builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<TData>(this, OnChange));
                    builder.AddAttribute(4, "OnBlur", EventCallback.Factory.Create<FocusEventArgs>(this, OnBlured));
                    builder.AddAttribute(5, "type", InputType);
                    builder.AddAttribute(6, "Disable", Disabled);

                    if (MaxValue is not null)
                    {
                        builder.AddAttribute(6, "Max", MaxValue);
                    }
                    else
                    {
                        var maxValue = typeof(TData).GetField("MaxValue")?.GetValue(Field) ?? 0;
                        builder.AddAttribute(6, "Max", maxValue);
                    }

                    if (MinValue is not null)
                    {
                        builder.AddAttribute(7, "Min", MinValue);
                    }
                    else
                    {
                        var minValue = typeof(TData).GetField("MinValue")?.GetValue(Field) ?? 0;
                        builder.AddAttribute(6, "Min", minValue);
                    }

                    if (Precision != null)
                    {
                        builder.AddAttribute(8, "Precision", Precision);
                    }

                    builder.AddAttribute(9, "style", "width:100%;");
                    builder.CloseComponent();
                }
                else
                {
                    builder.OpenComponent<Input<TData>>(0);
                    builder.AddAttribute(1, "TValue", typeof(TData));
                    builder.AddAttribute(2, "Value", Field);
                    builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<TData>(this, OnChange));
                    builder.AddAttribute(4, "OnBlur", EventCallback.Factory.Create<FocusEventArgs>(this, OnBlured));
                    builder.AddAttribute(5, "type", InputType);
                    builder.AddAttribute(5, "Width", "100%");

                    if (!string.IsNullOrWhiteSpace(Prefix))
                    {
                        builder.AddContent(0, new MarkupString($"<Prefix>{Prefix}</Prefix>"));
                    }

                    builder.CloseComponent();
                }
            }
            else
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "editable-cell");
                builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnCellClick));

                const string emptyValue = "-";
                var displayValue = Formatter(Field) ?? emptyValue;
                if (displayValue == emptyValue)
                {
                    builder.AddAttribute(4, "style", "text-align: center;");
                }

                if (!string.IsNullOrWhiteSpace(Prefix))
                {
                    builder.AddContent(3, Prefix + displayValue);
                }
                else
                {
                    builder.AddContent(3, displayValue);
                }

                builder.CloseElement();
            }
        };
    }

    private static string? FormatDecimal(decimal? value)
    {
        if (value == null)
        {
            return null;
        }

        return value == Math.Truncate(value.Value)
            ? value.Value.ToString("0")
            : value.Value.ToString("0.00");
    }
}