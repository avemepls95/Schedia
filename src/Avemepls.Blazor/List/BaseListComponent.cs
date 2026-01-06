using AntDesign;
using AntDesign.TableModels;

using Avemepls.Blazor.MediatR;
using Avemepls.Blazor.Navigation;
using Avemepls.Core.Extensions;
using Avemepls.Core.Models;
using Avemepls.Domain.Commands;
using Avemepls.Domain.Queries;
using Avemepls.Mvc.StateProviders.Filter;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;

namespace Avemepls.Blazor.List;

/// <summary>
/// Base component used to display filterable and paged list of entities
/// </summary>
#pragma warning disable SA1619
public abstract partial class BaseListComponent<TModel, TEntity, TQuery> : ComponentBase, IDisposable
#pragma warning restore SA1619
    where TEntity : class, IHasId<TEntity>
    where TModel : class
    where TQuery : SearchQuery<TModel>, new()
{
    [Inject]
    protected IScopedMediator Mediator { get; set; }

    [Inject]
    protected MessageService Messages { get; set; }

    [Inject]
    protected IFilterStateProvider FilterStateProvider { get; set; }

    [CascadingParameter(Name = "PageType")]
    public Type? PageType { get; set; }

    [Inject]
    protected NavigationHistoryManager? NavigationHistoryManager { get; set; }

    [Inject]
    protected IStringLocalizer<BaseListComponent<TModel, TEntity, TQuery>> Loc { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Parameter]
    public EventCallback<PagedResponse<TModel>> OnItemsLoaded { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    protected int PageSize { get; set; } = 10;

    /// <summary>
    /// Current page index
    /// </summary>
    protected int PageIndex { get; set; } = 1;

    /// <summary>
    /// Total items in result
    /// </summary>
    protected int Total { get; set; }

    /// <summary>
    /// Items that is loaded
    /// </summary>
    public PagedResponse<TModel>? Items { get; protected set; }

    /// <summary>
    /// True when data is loading
    /// </summary>
    public bool IsLoading { get; protected set; } = true;

    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// Query string that is used to pass query string in filter (same as Filter.Query)
    /// </summary>
    protected string? Query
    {
        get => Filter?.Query;
        set => Filter.Query = value;
    }

    /// <summary>
    /// Filter that is used to load data
    /// </summary>
    protected TQuery Filter { get; private set; } = new();

    /// <summary>
    /// Reloads page (and resets page index)
    /// </summary>
    public virtual async Task Reload()
    {
        await ReloadInternal(true);
    }

    /// <summary>
    /// Refreshes page in current state (keeps filters, paging etc)
    /// </summary>
    public virtual async Task Refresh()
    {
        await ReloadInternal(false);
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        NavigationManager.LocationChanged += OnLocationChanged;
        base.OnInitialized();
    }

    private async Task ReloadInternal(bool resetPaging)
    {
        try
        {
            IsLoading = true;

            if (resetPaging)
            {
                Filter.Offset = 0;
                Filter.Limit = PageSize;
                PageIndex = 1;
            }

            await FilterStateProvider.Persist(FilterPrefix, Filter);
            Items = await Mediator.Send(Filter);
            Total = (int)Items.Count;
            await OnItemsLoaded.InvokeAsync(Items);
            await OnAfterReload();
        }
        finally
        {
            IsLoading = false;
        }

        StateHasChanged();
    }

    /// <summary>
    /// Fires when list data is loaded.
    /// </summary>
    protected virtual Task OnAfterReload()
    {
        return Task.CompletedTask;
    }

    protected async Task DeleteItem(Id<TEntity> itemId)
    {
        try
        {
            await Mediator.Send(new DeleteCommand<TEntity>(itemId));
            await ReloadInternal(false);
            await Messages.SuccessAsync(Loc["Успешно удалено"].Value);
        }
        catch (FluentValidation.ValidationException e)
        {
            await Messages.ErrorAsync(string.Join("\n", e.Errors));
        }
        catch (Exception e)
        {
            await Messages.ErrorAsync(Loc["Ошибка при удалении: "] + e.Message);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                IsLoading = true;
                Filter = await BuildDefaultQuery();

                if (!await FilterStateProvider.Restore(FilterPrefix, Filter))
                {
                    await InitializeFilterWithDefaultParameters(Filter);
                }

                await OnAfterFilterRestoredAsync(Filter);
                StateHasChanged();

                if (Filter.Limit != null && Filter.Limit != 0)
                {
                    PageIndex = (int)(Filter.Offset / Filter.Limit) + 1;
                    PageSize = Filter.Limit ?? 10;
                }

                await ReloadInternal(false);
                IsInitialized = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Initializes filter with some defaults. This will be called on filter cleaning or when no parameters are passed from storage (query string),
    /// and filter should have some default state.
    /// </summary>
    /// <param name="filter">New instance of filter</param>
    protected virtual Task InitializeFilterWithDefaultParameters(TQuery filter) => Task.CompletedTask;

    /// <summary>
    /// Firest after filter is restored from filter storage (query etc). Should be used to set default unchangeable filter parameters.
    /// </summary>
    /// <param name="filter">Instance of filter in restored state</param>
    protected virtual Task OnAfterFilterRestoredAsync(TQuery filter) => Task.CompletedTask;

    /// <summary>
    /// Creates new query with base parametes.
    /// </summary>
    protected virtual Task<TQuery> BuildDefaultQuery() => Task.FromResult(
        new TQuery
        {
            Offset = (PageIndex - 1) * PageSize,
            Limit = PageSize,
            IncludeNonActive = true
        });

    protected async Task PageChanged(PaginationEventArgs arg)
    {
        PageIndex = arg.Page;
        PageSize = arg.PageSize;

        Filter.Offset = (arg.Page - 1) * arg.PageSize;
        Filter.Limit = arg.PageSize;

        await FilterStateProvider.Persist(FilterPrefix, Filter);
        await ReloadInternal(false);
    }

    /// <summary>
    /// Clear filter values
    /// </summary>
    protected async Task ClearFilter()
    {
        Filter = await BuildDefaultQuery();
        await InitializeFilterWithDefaultParameters(Filter);
        await OnAfterFilterRestoredAsync(Filter);
        await FilterStateProvider.Persist(FilterPrefix, Filter);
        await ReloadInternal(true);
    }

    private int _sortFieldsHashCode;

    /// <summary>
    /// Helping handler for tracking changes on table sorting or filtering. Use this method when table sort or filtering conditions changed.
    /// </summary>
    protected virtual async Task OnTableChange(QueryModel<TModel> queryModel)
    {
        var sortableFields = queryModel.SortModel.Where(x => x.SortDirection != null).ToArray();

        var sortFieldsHashCode = sortableFields.Select(x => x.FieldName + x.SortDirection).GetOrderIndependentHashCode();

        if (_sortFieldsHashCode == sortFieldsHashCode)
        {
            return;
        }

        _sortFieldsHashCode = sortFieldsHashCode;

        Filter.ClearSort();

        foreach (var sortModel in sortableFields)
        {
            var isDescending = sortModel.SortDirection != SortDirection.Ascending;
            Filter.OrderBy(sortModel.FieldName, isDescending);
        }

        if (IsInitialized)
            await Refresh();
    }

    /// <summary>
    /// Prefix used to persist filter state in storage. If there are many filters on same page, they should have different prefixes.
    /// </summary>
    public virtual string? FilterPrefix { get; set; }

    /// <summary>
    /// Called when page size changed by user, causing data to reload regarding new page size
    /// </summary>
    /// <param name="pageSize">New page size</param>
    protected async Task PageSizeChanged(int pageSize)
    {
        await PageChanged(new PaginationEventArgs(PageIndex, pageSize));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (PageType != null && NavigationHistoryManager != null)
        {
            await NavigationHistoryManager.AddPageToHistory(
                new NavigationHistoryRecord(PageType.FullName!, NavigationManager!.Uri));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}