using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Extensions;
using Avemepls.Domain.Filters;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Queries;

public abstract class GetEntityByIdQueryHandler<TQuery, TModel, TEntity, TContext>(
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
public abstract class GetEntityByIdQueryHandler<TQuery, TModel, TEntity, TContext, TId>(
    IMapper mapper,
    TContext context,
    IEnumerable<IQueryableModifier<TEntity>> modifiers) : IRequestHandler<TQuery, TModel>
#pragma warning restore SA1402
    where TContext : DbContext
    where TEntity : class, IHasId<TId>
    where TQuery : GetEntityByIdQuery<TModel, TId>
{
    /// <summary>
    /// Контекст работы с БД.
    /// </summary>
    protected TContext Context { get; } = context;

    /// <summary>
    /// Маппер сущностей.
    /// </summary>
    protected IMapper Mapper { get; } = mapper;

    /// <summary>
    /// Модификаторы запроса.
    /// </summary>
    protected IEnumerable<IQueryableModifier<TEntity>> Modifiers { get; } = modifiers;

    /// <inheritdoc />
    public virtual async Task<TModel> Handle(TQuery request, CancellationToken cancellationToken)
    {
        return await Get(request, cancellationToken) ??
               throw new ObjectNotFoundException<TId>(typeof(TEntity), request.Id);
    }

    /// <summary>
    /// Получить сущность по идентификатору.
    /// </summary>
    protected virtual async Task<TModel?> Get(TQuery query, CancellationToken cancellationToken)
    {
        var queryable = Context.Set<TEntity>()
            .AsQueryable();

        queryable = Include(queryable);

        if (!query.IgnoreAllQueryableModifiers)
        {
            queryable = await queryable.ApplyModifiers(Modifiers, cancellationToken);
        }

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
    /// Maps queryable with single entity (already filtered by it's ID) to model using projection.
    /// Can be overriden for complex cases when include is needed
    /// </summary>
    protected virtual async Task<TModel?> MapQueryableToModel(
        IQueryable<TEntity> queryable,
        CancellationToken cancellationToken)
    {
        return await Mapper
            .ProjectTo<TModel>(queryable)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Включить необходимый навигационные свойства.
    /// </summary>
    protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query) => query;
}