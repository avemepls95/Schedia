using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Поисковый запрос для получения списка сущностей.
/// </summary>
/// <typeparam name="TModel">Тип модели</typeparam>
public abstract class ListQuery<TModel>(bool includeDeleted = false, bool includeNonActive = false)
    : ListQuery<TModel, int, PagedResponse<TModel>>(includeDeleted, includeNonActive)
{
}

/// <summary>
/// Поисковый запрос для получения списка сущностей.
/// </summary>
/// <typeparam name="TModel">Тип модели</typeparam>
/// <typeparam name="TId">Тип идентификатора сущностей</typeparam>
#pragma warning disable SA1402
public abstract class ListQuery<TModel, TId>(bool includeDeleted = false, bool includeNonActive = false)
    : ListQuery<TModel, TId, PagedResponse<TModel>>(includeDeleted, includeNonActive)
#pragma warning restore SA1402
{
}

/// <summary>
/// Поисковый запрос для получения списка сущностей.
/// </summary>
/// <typeparam name="TModel">Тип модели</typeparam>
/// <typeparam name="TId">Тип идентификатора сущностей</typeparam>
/// <typeparam name="TResult">Тип результата запроса, если нужно расширить PagedResponse</typeparam>
[DebuggerDisplay("Offset and limit {Offset}:{Limit} (ignore all modifiers: {IgnoreAllQueryableModifiers})")]
#pragma warning disable SA1402
public abstract class ListQuery<TModel, TId, TResult>(bool includeDeleted = false, bool includeNonActive = false)
    : LimitQuery, IRequest<TResult>, IListQuery<TId>
    where TResult : PagedResponse<TModel>, new()
#pragma warning restore SA1402
{
    /// <summary>
    /// Перечень идентификаторов сущностей для фильтрации по айди
    /// </summary>
    public TId[]? Ids { get; set; }

    /// <summary>
    /// Перечень идентификаторов сущностей, которые нужно исключить из выдачи
    /// </summary>
    public TId[]? ExcludeIds { get; set; }

    /// <summary>
    /// Показывать в начале
    /// </summary>
    public TId[]? SortByIds { get; set; }

    /// <summary>
    /// Порядок сортировки данных. Если не указан, данные не будут сортированы, либо будут отсортированы по дате создания,
    /// если сущность реализует интерфейс <see cref="IHasDateCreated"/>. Чтобы отсортировать в порядке убывания,
    /// используйте "-" в начале названия свойства. Например:
    /// `OrderBy=name,-createdDate`
    /// </summary>
    public string[]? OrderBy { get; set; }

    /// <summary>
    /// Список модификаторов, которые следует игнорировать при выборке данных
    /// </summary>
    internal HashSet<Type>? IgnoreQueryableModifiers { get; set; }

    /// <summary>
    /// Определяет, следует ли игнорировать вообще все модификаторы при выборке
    /// </summary>
    internal bool IgnoreAllQueryableModifiers { get; set; }

    /// <summary>
    /// Показывать удаленные
    /// </summary>
    /// <value>Default: false</value>
    public bool IncludeDeleted { get; set; } = includeDeleted;

    /// <summary>
    /// Показывать неактивные
    /// </summary>
    /// <value>Default: false</value>
    public bool IncludeNonActive { get; set; } = includeNonActive;
}