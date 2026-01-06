using AntDesign;
using AntDesign.Core.Helpers.MemberPath;

using Avemepls.Core.Models;
using Avemepls.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Components;

namespace Avemepls.Blazor.Components.Forms;

/// <summary>
/// Base class for QuerySelect components
/// </summary>
#pragma warning disable SA1619
public partial class QuerySelectBase<TItem, TItemValue, TQuery> : AntInputComponentBase<TItemValue>
#pragma warning restore SA1619
    where TQuery : SearchQuery<TItem>, new()
    where TItem : class
{
    private TQuery? _previousFilter;

    /// <summary>
    /// Template for select item.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem> ItemTemplate { get; set; }

    /// <summary>
    /// Is used to customize the label style.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem> LabelTemplate { get; set; }

    /// <summary>
    /// Limit search results.
    /// </summary>
    [Parameter]
    public int Limit { get; set; } = 20;

    /// <summary>
    /// Initial value for select control.
    /// </summary>
    [Parameter]
    public TItem? InitialValue { get; set; }

    /// <summary>
    /// Initial value for select control.
    /// </summary>
    [Parameter]
    public TItem[]? InitialValues { get; set; }

    /// <summary>
    /// Will match drowdown width
    /// </summary>
    [Parameter]
    public bool DropdownMatchSelectWidth { get; set; }

    /// <summary>
    /// The name of the property to be used for the value.
    /// </summary>
    [Parameter]
    public string ValueName { get; set; } = "Id";

    /// <summary>
    /// The name of the property to be used for the label.
    /// </summary>
    [Parameter]
    public string LabelName { get; set; } = "Name";

    /// <summary>
    /// Placeholder text.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; }

    /// <summary>
    /// Placeholder text.
    /// </summary>
    [Parameter]
    public SelectMode Mode { get; set; }

    /// <summary>
    /// Name in DOM-tree.
    /// </summary>
    [Parameter]
    public string DomName { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ShowArrowIcon { get; set; } = true;

    [Parameter]
    public bool AllowClear { get; set; }

    [Parameter]
    public bool EnableSearch { get; set; } = true;

    [Parameter]
    public IEqualityComparer<TQuery>? FilterComparer { get; set; }

    [Parameter]
    public EventCallback<PagedResponse<TItem>> OnItemsLoaded { get; set; }

    /// <summary>
    /// Select the first value with pre-initialization
    /// </summary>
    [Parameter]
    public bool DefaultActiveFirstOption { get; set; }

    /// <summary>
    /// Loading indicator.
    /// </summary>
    protected bool IsLoading { get; set; }

    /// <summary>
    /// Modifier for search request.
    /// </summary>
    [Parameter]
    public Action<TQuery>? QueryModifier { get; set; }

    /// <summary>
    /// Called when the selected item changes.
    /// </summary>
    [Parameter]
    public Action<TItem> OnSelectedItemChanged { get; set; }

    /// <summary>
    /// Called when the selected items changes.
    /// </summary>
    [Parameter]
    public Action<TItem[]>? OnSelectedItemsChanged { get; set; }

    [Inject]
    public IMediator Mediator { get; set; }

    protected TItem[] Items { get; set; } = [];

    public IReadOnlyCollection<TItem> InnerItems => Items;

    public bool IsMultiple => Mode is not SelectMode.Default;

    private object? _dependsOn;

    /// <summary>
    /// Ссылка на параметр, учавствующий в поиске.
    /// </summary>
    [Parameter]
    public object? DependsOn
    {
        get => _dependsOn;
        set
        {
            if (_dependsOn?.Equals(value) != true)
            {
                _dependsOn = value;

                if (_isInitialized)
                {
                    Items = [];
                    Value = default!;
                    _isItemsLoaded = false;
                    _dependenceChanged = true;
                }
            }
            else if (_dependsOn.Equals(value))
            {
                return;
            }
            else
            {
                _dependsOn = value;
            }

            if (_dependsOn is not null)
            {
                InvokeAsync(async () => await EnsureInitialItemLoaded(false));
            }
        }
    }

    [Parameter]
    public Func<TQuery> QueryFactory { get; set; } = () => new TQuery();

    private bool _isDepended;
    private bool _dependenceChanged;
    private bool _isInitialized;

    private TItemValue _value;

    /// <summary>
    /// Get or set the selected value.
    /// </summary>
    [Parameter]
#pragma warning disable BL0007
    public override TItemValue Value
#pragma warning restore BL0007
    {
        get => _value;
        set
        {
            if (_dependenceChanged) return;

            var valueHasChanged = !EqualityComparer<TItemValue>.Default.Equals(value, _value);

            if (valueHasChanged)
            {
                _value = value;
                InvokeAsync(async () => await ValueChanged.InvokeAsync(value));
            }
        }
    }

    [Parameter]
    public EventCallback<TItemValue[]?> ValuesChanged { get; set; }

    private TItemValue[]? _values;

    /// <summary>
    /// Get or set the selected values.
    /// </summary>
    [Parameter]
    public TItemValue[]? Values
    {
        get => _values;
        set
        {
            var valueHasChanged = !Equals(_values, value);

            if (valueHasChanged)
            {
                _values = value;
                InvokeAsync(async () => await ValuesChanged.InvokeAsync(value));
            }
        }
    }

    private bool _isItemsLoaded;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Items.Any())
            {
                await EnsureInitialItemLoaded();
            }

            _isInitialized = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Name == nameof(DependsOn))
            {
                _isDepended = true;

                break;
            }
        }

        await base.SetParametersAsync(parameters);
    }

    private void OnSearch(string? query = null)
    {
        if (!EnableSearch)
            return;

        InvokeAsync(async () => await ReloadItems(query));
    }

    /// <summary>
    /// Loads top items regarding query.
    /// </summary>
    /// <param name="query">Query text to search items for.</param>
    public async Task ReloadItems(string? query = null)
    {
        try
        {
            _dependenceChanged = false;
            IsLoading = true;

            var request = QueryFactory();

            request.Limit = EnableSearch
                ? Limit
                : null;

            request.Query = query;

            QueryModifier?.Invoke(request);

            if (typeof(TItemValue) == typeof(Id<TItem>) && IsMultiple && Values?.Any() == true &&
                string.IsNullOrWhiteSpace(query))
            {
                request.AddSortByIds(Values.Cast<Id<TItem>>().ToArray());
                request.Limit += Values.Length;
            }

            var response = await Mediator.Send(request);

            var withValue = IsMultiple
                ? !Equals(Values, null) && Values?.Any() == true
                : !Equals(Value, default(TItemValue));

            if (withValue)
            {
                var selectedValues = IsMultiple
                    ? Values!
                    : [Value];

                if (!string.IsNullOrEmpty(ValueName))
                {
                    var getValue = PathHelper.GetDelegate<TItem, TItemValue>(ValueName);
                    var selectedItems = Items.Where(item => selectedValues.Contains(getValue.Invoke(item)));
                    var newItems = response.Results.Where(item => !selectedValues.Contains(getValue.Invoke(item)));
                    Items = selectedItems.Union(newItems).ToArray();
                }
                else
                {
                    var selectedItems = Items.Where(item => selectedValues.Cast<TItem>().Contains(item));
                    var newItems = response.Results.Where(item => !selectedValues.Cast<TItem>().Contains(item));
                    Items = selectedItems.Union(newItems).ToArray();
                }
            }
            else
            {
                Items = response.Results;
            }

            _isItemsLoaded = true;

            await OnItemsLoaded.InvokeAsync(response);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Loads only one initial selected item
    /// </summary>
    public async Task EnsureInitialItemLoaded(bool setInitialValue = true)
    {
        if (_isDepended && (DependsOn is null || DependsOn.Equals(null)))
        {
            return;
        }

        try
        {
            if (setInitialValue)
            {
                if (InitialValue is not null && !IsMultiple)
                {
                    Items = [InitialValue];

                    return;
                }

                if (InitialValues is not null)
                {
                    Items = InitialValues.ToArray();

                    return;
                }
            }

            var withoutValue = IsMultiple
                ? Equals(Values, null)
                : Equals(Value, default(TItemValue));

            if (withoutValue && !DefaultActiveFirstOption)
            {
                return;
            }

            _dependenceChanged = false;
            IsLoading = true;

            var request = QueryFactory();
            request.Limit = 1;

            if (!withoutValue)
            {
                request.Ids = IsMultiple
                    ? Values!.Select(x => (Id<TItem>)Convert.ChangeType(x, typeof(Id<TItem>))!).ToArray()
                    : [(Id<TItem>)Convert.ChangeType(Value, typeof(Id<TItem>))!];

                request.Limit = request.Ids.Length;
            }

            if (DefaultActiveFirstOption)
            {
                QueryModifier?.Invoke(request);
            }
            else
            {
                request.IgnoreQueryableModifiers();
            }

            var response = await Mediator.Send(request);

            Items = response.Results;

            if (Items.Any())
            {
                var getValue = PathHelper.GetDelegate<TItem, TItemValue>(ValueName);

                Value = getValue(Items[0]);
            }
        }
        finally
        {
            IsLoading = false;

            StateHasChanged();
        }
    }

    private void OnFocus()
    {
        if (Disabled)
        {
            return;
        }

        var filter = QueryFactory();
        QueryModifier?.Invoke(filter);

        var filterHasChanges = FilterComparer?.Equals(_previousFilter, filter) == false;

        if (_isItemsLoaded && !filterHasChanges)
            return;

        _previousFilter = filter;

        InvokeAsync(async () => await ReloadItems());
    }

    private void OnDropdownVisibleChange(bool isVisible)
    {
        if (Mode is SelectMode.Multiple && !isVisible)
        {
            OnSearch();
        }
    }

    private void SelectedItemsChanged(IEnumerable<TItem>? items)
    {
        if (_isInitialized)
        {
            OnSelectedItemsChanged?.Invoke(items?.ToArray() ?? []);
        }
    }
}