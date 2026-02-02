using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Extensions;
using Avemepls.Domain.Filters;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Queries;

public abstract class GetEntityByIdPostMappedQueryHandler<TQuery, TModel, TEntity, TContext>(
    IMapper mapper,
    TContext context,
    IEnumerable<IQueryableModifier<TEntity>> modifiers)
    : GetEntityByIdQueryHandler<TQuery, TModel, TEntity, TContext, int>(mapper, context, modifiers)
    where TContext : DbContext
    where TEntity : class, IHasId
    where TQuery : GetEntityByIdQuery<TModel, int>;

/// <summary>
/// Get model by ID
/// </summary>
/// <typeparam name="TQuery">Query type</typeparam>
/// <typeparam name="TModel">Type of model object</typeparam>
/// <typeparam name="TEntity">Type of entity</typeparam>
/// <typeparam name="TContext">Data context</typeparam>
/// <typeparam name="TId">Type of identifier</typeparam>
#pragma warning disable SA1402
public abstract class GetEntityByIdPostMappedQueryHandler<TQuery, TModel, TEntity, TContext, TId> : IRequestHandler<TQuery, TModel>
#pragma warning restore SA1402
    where TContext : DbContext
    where TEntity : class, IHasId<TId>
    where TQuery : GetEntityByIdQuery<TModel, TId>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    protected GetEntityByIdPostMappedQueryHandler(
        IMapper mapper,
        TContext context,
        IEnumerable<IQueryableModifier<TEntity>> modifiers)
    {
        Mapper = mapper;
        Context = context;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Контекст работы с БД.
    /// </summary>
    protected TContext Context { get; }

    /// <summary>
    /// Маппер сущностей.
    /// </summary>
    protected IMapper Mapper { get; }

    /// <summary>
    /// Модификаторы запроса.
    /// </summary>
    protected IEnumerable<IQueryableModifier<TEntity>> Modifiers { get; }

    /// <inheritdoc />
    public virtual async Task<TModel> Handle(TQuery request, CancellationToken cancellationToken)
    {
        return await Get(request, cancellationToken) ?? throw new ObjectNotFoundException<TId>(typeof(TEntity), request.Id);
    }

    /// <summary>
    /// Получить сущность по идентификатору.
    /// </summary>
    protected virtual async Task<TModel?> Get(TQuery query, CancellationToken cancellationToken)
    {
        var queryable = Include(Context.Set<TEntity>().AsQueryable().AsNoTracking());

        if (!query.IgnoreAllQueryableModifiers)
            queryable = await queryable.ApplyModifiers(Modifiers, cancellationToken);

        if (!query.IncludeDeleted)
        {
            queryable = queryable.WhereIsNotDeleted();
        }

        if (!query.IncludeNonActive)
        {
            queryable = queryable.WhereIsActive();
        }

        return await MapQueryableToModel(queryable.Where(e => e.Id!.Equals(query.Id)), cancellationToken);
    }

    /// <summary>
    /// Maps queryable with single entity (already filtered by it's ID) to model. Data is firt materialized in entity and then mapped to model.
    /// </summary>
    protected virtual async Task<TModel?> MapQueryableToModel(IQueryable<TEntity> queryable, CancellationToken cancellationToken)
    {
        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return default;

        return Mapper.Map<TModel>(entity);
    }

    /// <summary>
    /// Включить необходимый навигационные свойства.
    /// </summary>
    protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query) => query;
}