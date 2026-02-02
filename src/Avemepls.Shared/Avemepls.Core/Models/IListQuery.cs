namespace Avemepls.Core.Models;

public interface IListQuery<TId> : IHasIncludeDeleted, IHasIncludeNonActive, ILimitQuery
{
    /// <summary>
    /// Перечень идентификаторов сущностей для фильтрации по айди
    /// </summary>
    public TId[]? Ids { get; set; }

    /// <summary>
    /// Перечень идентификаторов сущностей, которыйх нужно исключить из выдачи
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
}

public interface IListQuery : IListQuery<int>
{
}