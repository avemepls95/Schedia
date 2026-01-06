using System.Linq.Expressions;

using AntDesign;
using AntDesign.Internal;

using Avemepls.Core.Extensions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Avemepls.Blazor.Components;

/// <summary>
/// DateRange picker component
/// </summary>
#pragma warning disable SA1619
public partial class DateRangePicker<TValue>
#pragma warning restore SA1619
{
    private TValue[]? _defaultValues;
    private IEnumerable<string> _validationMessages = [];

    [Inject]
    private IServiceProvider ServiceProvider { get; set; }

    [Inject]
    private IStringLocalizer<DateRangePicker<TValue>> Loc { get; set; }

    private TValue _endDate;
    private TValue _startDate;
#pragma warning disable SA1401
    public TValue[] Value { get; private set; }
#pragma warning restore SA1401

    [Parameter]
    public string[]? Placeholder { get; set; }

    [Parameter]
    public bool AllowClear { get; set; }

    [Parameter]
    public IFormItem FormItem { get; set; }

    [Parameter]
#pragma warning disable BL0007
    public TValue StartDate
#pragma warning restore BL0007
    {
        get => _startDate;
        set
        {
            if (_startDate?.Equals(value) is true || (_startDate is null && value is null))
            {
                return;
            }

            _startDate = value;

            StartDateChanged.InvokeAsync(value!);

            if (StartDateFieldIdentifier is { Model: not null, FieldName: not null })
            {
                CascadedEditContext?.NotifyFieldChanged(StartDateFieldIdentifier.Value);
            }
        }
    }

    [Parameter]
    public Func<DateTime, DateTime, RenderFragment>? DateRender { get; set; }

    [Parameter]
    public EventCallback<TValue> StartDateChanged { get; set; }

    [Parameter]
#pragma warning disable BL0007
    public TValue EndDate
#pragma warning restore BL0007
    {
        get => _endDate;
        set
        {
            if (_endDate?.Equals(value) is true || (_endDate is null && value is null))
            {
                return;
            }

            _endDate = value;

            Value = [StartDate, EndDate];
            EndDateChanged.InvokeAsync(value);

            if (EndDateFieldIdentifier is { Model: not null, FieldName: not null })
            {
                CascadedEditContext?.NotifyFieldChanged(EndDateFieldIdentifier.Value);
            }
        }
    }

    [Parameter]
    public EventCallback<TValue> EndDateChanged { get; set; }

    private async Task ChangeValue(DateRangeChangedEventArgs<TValue[]> arg)
    {
        await StartDateChanged.InvokeAsync(arg.Dates[0]);
        await EndDateChanged.InvokeAsync(arg.Dates[^1]);
    }

    [Parameter]
    public Func<DateTime, DateTime, string>? DateRenderStyle { get; set; }

    [Parameter]
    public TValue? StartDateDefaultValue { get; set; }

    [Parameter]
    public TValue? EndDateDefaultValue { get; set; }

    [Parameter]
    public Expression<Func<TValue>> EndDateExpression { get; set; }

    [Parameter]
    public Expression<Func<TValue>> StartDateExpression { get; set; }

    public FieldIdentifier? StartDateFieldIdentifier { get; set; }

    public FieldIdentifier? EndDateFieldIdentifier { get; set; }

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Placeholder is null || Placeholder.IsEmpty())
        {
            Placeholder = [Loc["С"], Loc["По"]];
        }

        var defaultValues = new List<TValue>(new TValue[2]);

        if (StartDateDefaultValue is not null)
        {
            defaultValues[0] = StartDateDefaultValue;
            StartDate = StartDateDefaultValue;
        }

        if (EndDateDefaultValue is not null)
        {
            defaultValues[1] = EndDateDefaultValue;
            EndDate = EndDateDefaultValue;
        }

        if (StartDateFieldIdentifier is null && StartDateExpression is not null)
        {
            StartDateFieldIdentifier = FieldIdentifier.Create(StartDateExpression);
        }

        if (EndDateFieldIdentifier is null && EndDateExpression is not null)
        {
            StartDateFieldIdentifier = FieldIdentifier.Create(EndDateExpression);
        }

        if (CascadedEditContext is not null)
        {
            CascadedEditContext.OnValidationStateChanged += ValidationStateChanged;
        }

        _defaultValues = defaultValues.ToArray();
    }

    private void ValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        var messages = new List<string>();

        if (StartDateFieldIdentifier is not null)
        {
            var errors = CascadedEditContext!.GetValidationMessages(StartDateFieldIdentifier.Value);
            messages.AddRange(errors);
        }

        if (EndDateFieldIdentifier is not null)
        {
            var errors = CascadedEditContext!.GetValidationMessages(EndDateFieldIdentifier.Value);
            messages.AddRange(errors);
        }

        _validationMessages = messages.ToArray();
    }
}