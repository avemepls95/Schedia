using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Extensions;
using Avemepls.Domain.Filters;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Обработчик запроса на поиск элементов. Отличается от <see cref="ListQueryHandler{TModel,TEntity,TContext,TListQuery}"/> особым способом
/// материализации моделей - из хранилища сначала материализуются сущности, затем происходит маппинг в модели.
/// </summary>
/// <typeparam name="TModel">Тип модели.</typeparam>
/// <typeparam name="TEntity">Тип сущности в БД.</typeparam>
/// <typeparam name="TContext">Тип контекста БД.</typeparam>
/// <typeparam name="TListQuery">Тип поискового запроса.</typeparam>
#pragma warning disable SA1402
public abstract class ListQueryPostMappedHandler<TModel, TEntity, TContext, TListQuery>
#pragma warning restore SA1402
(
    IMapper mapper,
    IDbContextFactory<TContext> context,
    IEnumerable<IQueryableModifier<TEntity>>? modifiers)
    : ListQueryPostMappedHandler<TModel, TEntity, TContext, TListQuery, PagedResponse<TModel>>(
        mapper,
        context,
        modifiers)
    where TEntity : class, IHasId<TEntity>
    where TModel : class
    where TContext : DbContext
    where TListQuery : ListQuery<TModel>
{
}

/// <summary>
/// Обработчик запроса на поиск элементов. Отличается от <see cref="ListQueryHandler{TModel,TEntity,TContext,TListQuery}"/> особым способом
/// материализации моделей - из хранилища сначала материализуются сущности, затем происходит маппинг в модели.
/// </summary>
/// <typeparam name="TModel">Тип модели.</typeparam>
/// <typeparam name="TEntity">Тип сущности в БД.</typeparam>
/// <typeparam name="TContext">Тип контекста БД.</typeparam>
/// <typeparam name="TListQuery">Тип поискового запроса.</typeparam>
/// <typeparam name="TResult">Тип результата запроса, если нужно расширить PagedResponse</typeparam>
#pragma warning disable SA1402
public abstract class ListQueryPostMappedHandler<TModel, TEntity, TContext, TListQuery, TResult>
#pragma warning restore SA1402
(
    IMapper mapper,
    IDbContextFactory<TContext> contextFactory,
    IEnumerable<IQueryableModifier<TEntity>>? modifiers) : IRequestHandler<TListQuery, TResult>
    where TEntity : class, IHasId<TEntity>
    where TModel : class
    where TContext : DbContext
    where TResult : PagedResponse<TModel>, new()
    where TListQuery : ListQuery<TModel, TResult>
{
    /// <summary>
    /// Маппер сущностей.
    /// </summary>
    protected IMapper Mapper { get; } = mapper;

    /// <summary>
    /// Контекст работы с БД.
    /// </summary>
    protected IDbContextFactory<TContext> ContextFactory { get; } = contextFactory;

    /// <summary>
    /// Модификаторы запроса.
    /// </summary>
    protected IEnumerable<IQueryableModifier<TEntity>>? Modifiers { get; } = modifiers;

    /// <inheritdoc />
    public virtual async Task<TResult> Handle(TListQuery request, CancellationToken cancellationToken)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        var query = await BuildQuery(context, request, cancellationToken);
        var count = await query.CountAsync(cancellationToken);
        query = Sort(request, query);

        var list = await MapQueryableToModel(request,
                                             count <= request.Limit
                                                 ? query
                                                 : query.Paging(request),
                                             cancellationToken);

        var response = new TResult
        {
            Results = list,
            Count = count
        };

        return await BuildResult(response, context, request, cancellationToken);
    }

    protected virtual async Task<TModel[]> MapQueryableToModel(
        TListQuery request,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
    {
        var materialized = await query.ToArrayAsync(cancellationToken);

        return Mapper.Map<TModel[]>(materialized);
    }

    /// <summary>
    /// Изменение результата запроса после выборки и маппинга объектов.
    /// Нужен при расширении модели результата запроса дополнительными полями
    /// </summary>
    /// <param name="result">Результат запроса</param>
    /// <param name="context">Контекст БД</param>
    /// <param name="request">Запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Измененный результат</returns>
    protected virtual Task<TResult> BuildResult(
        TResult result,
        TContext context,
        TListQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(result);
    }

    /// <summary>
    /// Добавить специфичные запросу фильтры.
    /// </summary>
    protected virtual async Task<IQueryable<TEntity>> BuildQuery(
        TContext context,
        TListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context
            .Set<TEntity>()
            .AsQueryable()
            .AsNoTracking();

        if (request.Ids is not null)
            query = query.Where(e => request.Ids.Select(x => x.Value).Contains(e.Id.Value));

        if (request.ExcludeIds is not null)
            query = query.Where(e => !request.ExcludeIds.Select(x => x.Value).Contains(e.Id.Value));

        if (!request.IncludeDeleted)
            query = query.WhereIsNotDeleted();

        if (!request.IncludeNonActive)
            query = query.WhereIsActive();

        query = await query.ApplyModifiers<TEntity, TModel, TListQuery, TResult>(
            Modifiers,
            request,
            cancellationToken);

        return query;
    }

    /// <summary>
    /// Добавляет сортировку к запросу по указанным полям (SortableFields); применяется в соответствующем порядке
    /// </summary>
    protected virtual IQueryable<TEntity> Sort(TListQuery request, IQueryable<TEntity> query)
    {
        var isNestedSort = false;

        if (request.SortByIds?.Any() == true)
        {
            query = query.OrderByDescending(x => request.SortByIds.Select(id => id.Value).Contains(x.Id.Value));

            isNestedSort = true;
        }

        if (request.OrderBy?.Any() == true)
        {
            foreach (var sortableField in request.OrderBy)
            {
                var (propertyName, descending) = sortableField.NormalizeSortPath();
                query = query.OrderByProperty(propertyName, descending, isNestedSort);

                isNestedSort = true;
            }

            return query;
        }

        if (isNestedSort)
        {
            return query;
        }

        return DefaultSort(request, query);
    }

    /// <summary>
    /// Default sorting when sort fields are empty
    /// </summary>
    protected virtual IQueryable<TEntity> DefaultSort(TListQuery request, IQueryable<TEntity> query)
    {
        if (typeof(TEntity).IsAssignableTo(typeof(IHasDateCreated)))
        {
            return query
                .OrderByProperty(nameof(IHasDateCreated.DateCreated), true)
                .ThenByDescending(q => q.Id);
        }

        return query.OrderBy(x => x.Id);
    }
}