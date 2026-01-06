using System.Linq.Expressions;

using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Extensions;
using Avemepls.Domain.Filters;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Обработчик запроса на поиск элементов.
/// </summary>
/// <typeparam name="TModel">Тип модели.</typeparam>
/// <typeparam name="TEntity">Тип сущности в БД.</typeparam>
/// <typeparam name="TContext">Тип контекста БД.</typeparam>
/// <typeparam name="TListQuery">Тип поискового запроса.</typeparam>
#pragma warning disable SA1402
public abstract class ListQueryHandler<TModel, TEntity, TContext, TListQuery>
#pragma warning restore SA1402
(
    IMapper mapper,
    IDbContextFactory<TContext> context,
    IEnumerable<IQueryableModifier<TEntity>>? modifiers)
    : ListQueryHandler<TModel, TEntity, TContext, TListQuery, PagedResponse<TModel>>(mapper, context, modifiers)
    where TEntity : class, IHasId<TEntity>
    where TContext : DbContext
    where TListQuery : ListQuery<TModel>
    where TModel : class, IHasId<TModel>

{
}

/// <summary>
/// Обработчик запроса на поиск элементов.
/// </summary>
/// <typeparam name="TModel">Тип модели.</typeparam>
/// <typeparam name="TEntity">Тип сущности в БД.</typeparam>
/// <typeparam name="TContext">Тип контекста БД.</typeparam>
/// <typeparam name="TListQuery">Тип поискового запроса.</typeparam>
/// <typeparam name="TResult">Тип результата запроса, если нужно расширить PagedResponse</typeparam>
#pragma warning disable SA1402
public abstract class ListQueryHandler<TModel, TEntity, TContext, TListQuery, TResult>
#pragma warning restore SA1402
(
    IMapper mapper,
    IDbContextFactory<TContext> contextFactory,
    IEnumerable<IQueryableModifier<TEntity>>? modifiers)
    : IRequestHandler<TListQuery, TResult>
    where TEntity : class, IHasId<TEntity>
    where TContext : DbContext
    where TListQuery : ListQuery<TModel, TResult>
    where TResult : PagedResponse<TModel>, new()
    where TModel : class, IHasId<TModel>
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

        var modelQuery = await MapQueryableToModel(request, query, cancellationToken);
        var sortedQueryModel = Sort(request, modelQuery);

        var list = count <= request.Limit
            ? await sortedQueryModel.ToArrayAsync(cancellationToken)
            : await sortedQueryModel.Paging(request).ToArrayAsync(cancellationToken);

        var response = new TResult
        {
            Results = list,
            Count = count
        };

        return await BuildResult(response, context, request, cancellationToken);
    }

    protected virtual Task<IQueryable<TModel>> MapQueryableToModel(
        TListQuery request,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Mapper.ProjectTo<TModel>(query));
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
        {
            query = query.WhereIsNotDeleted();
        }

        if (!request.IncludeNonActive)
        {
            query = query.WhereIsActive();
        }

        query = await query.ApplyModifiers<TEntity, TModel, TListQuery, TResult>(Modifiers, request, cancellationToken);

        return query.OrderBy(e => e.Id);
    }

    /// <summary>
    /// Добавляет сортировку к запросу по указанным полям (SortableFields); применяеться в соответствующем порядке
    /// </summary>
    protected virtual IQueryable<TModel> Sort(TListQuery request, IQueryable<TModel> query)
    {
        var isNestedSort = request.SortByIds?.Any() == true && TrySortByIds(ref query, request.SortByIds);

        query = query.Sort(request.OrderBy, isNestedSort);

        if (request.OrderBy?.Any() == true)
        {
            return query;
        }

        return DefaultSort(request, query);
    }

    private static bool TrySortByIds(ref IQueryable<TModel> source, IEnumerable<Id<TModel>> ids)
    {
        var propertyInfo = typeof(TModel).GetProperty("Id");

        if (propertyInfo == null)
            return false;

        var parameterExpression = Expression.Parameter(typeof(TModel), "p");
        var propertyAccess = Expression.MakeMemberAccess(parameterExpression, propertyInfo);
        Expression<Func<IEnumerable<int>, bool>> containsExpr = q => q.Contains(0);

        var containsMethod = ((MethodCallExpression)containsExpr.Body).Method;

        var containsCall = Expression.Call(containsMethod,
                                           Expression.Constant(ids),
                                           propertyAccess);

        var whereExpr = (Expression<Func<TModel, bool>>)Expression.Lambda(containsCall, parameterExpression);

        source = source.OrderByDescending(whereExpr);

        return true;
    }

    /// <summary>
    /// Default sorting when sort fields are empty
    /// </summary>
    protected virtual IQueryable<TModel> DefaultSort(TListQuery request, IQueryable<TModel> query)
    {
        if (typeof(TModel).IsAssignableTo(typeof(IHasSortOrder)))
        {
            return query.OrderByProperty(nameof(IHasSortOrder.SortOrder));
        }

        if (typeof(TModel).IsAssignableTo(typeof(IHasDateCreated)))
        {
            return query.OrderByProperty(nameof(IHasDateCreated.DateCreated), true);
        }

        return query;
    }
}