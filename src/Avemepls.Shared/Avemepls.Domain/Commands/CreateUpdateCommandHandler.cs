using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Security;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Commands;

public abstract class CreateUpdateCommandHandler<TCommand, TContext, TModel, TEntity>(
    TContext context,
    IMapper mapper,
    IEnumerable<IPermissionEvaluator<TEntity>> evaluators)
    : CreateUpdateCommandHandler<TCommand, TContext, TModel, TEntity, int>(context, mapper, evaluators)
    where TCommand : CreateUpdateCommand<int, TModel>
    where TContext : DbContext
    where TEntity : class, IHasId, new();

/// <summary>
/// Creates or updates existing entity
/// </summary>
#pragma warning disable SA1402
public abstract class CreateUpdateCommandHandler<TCommand, TContext, TModel, TEntity, TId>(
    TContext context,
    IMapper mapper,
    IEnumerable<IPermissionEvaluator<TEntity>> evaluators)
    : IRequestHandler<TCommand, TId>
#pragma warning restore SA1402
    where TCommand : CreateUpdateCommand<TId, TModel>
    where TContext : DbContext
    where TEntity : class, IHasId<TId>, new()
{
    /// <summary>
    /// Database context
    /// </summary>
    protected TContext Context { get; } = context;

    /// <summary>
    /// Mapper to map model to entity
    /// </summary>
    protected IMapper Mapper { get; } = mapper;

    /// <summary>
    /// Permission evaluators collection
    /// </summary>
    protected IEnumerable<IPermissionEvaluator<TEntity>> Evaluators { get; } = evaluators;

    /// <summary>
    /// Handles create/update operation
    /// </summary>
    public virtual async Task<TId> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await TryGet(request, cancellationToken);

        if (entity is null)
        {
            entity = await CreateEntity(request, cancellationToken);
            Context.Set<TEntity>().Add(entity);
            var id = entity.Id;
            await Map(request.Model, entity, cancellationToken);
            entity.Id = id;
            await ValidateInsertAccess(entity, cancellationToken);
        }
        else
        {
            await Map(request.Model, entity, cancellationToken);
            await ValidateUpdateAccess(entity, cancellationToken);
            if (request.Id is not 0)
            {
                entity.Id = request.Id!;
            }
        }

        await EntitySaving(request, entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        await EntitySaved(request, entity, cancellationToken);
        return entity.Id;
    }

    /// <summary>
    /// If entity is creating (not found in DB), this method used to create new instance of entity
    /// </summary>
    protected virtual ValueTask<TEntity> CreateEntity(TCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new TEntity());
    }

    /// <summary>
    /// Fires after entity changes saved to DB
    /// </summary>
    protected virtual Task EntitySaved(TCommand request, TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Map model data onto existing entity
    /// </summary>
    protected virtual Task Map(TModel model, TEntity entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(Mapper.Map(model, entity));
    }

    /// <summary>
    /// Try to load entity from DB. Default implementation loads entity by it's primary key Id
    /// </summary>
    protected virtual async Task<TEntity?> TryGet(TCommand request, CancellationToken cancellationToken)
    {
        if (request.Id is 0 or null)
            return null;

        var entity = await Include(Context.Set<TEntity>())
            .AsTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(request.Id), cancellationToken);
        if (entity == null)
            throw new ObjectNotFoundException<int>(typeof(TEntity), "" + request.Id);

        return entity;
    }

    /// <summary>
    /// Allows to include some properties to entity
    /// </summary>
    protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query) => query;

    /// <summary>
    ///  Fires before entity changes saved to DB
    /// </summary>
    protected virtual Task EntitySaving(TCommand request, TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validate access for entity create
    /// </summary>
    protected virtual async Task ValidateInsertAccess(TEntity entity, CancellationToken cancellationToken)
    {
        foreach (var evaluator in Evaluators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await evaluator.CanAdd(entity, cancellationToken))
            {
                throw new AccessDeniedException();
            }
        }
    }

    /// <summary>
    /// Validate access for entity update
    /// </summary>
    protected virtual async Task ValidateUpdateAccess(TEntity entity, CancellationToken cancellationToken)
    {
        foreach (var evaluator in Evaluators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await evaluator.CanUpdate(entity, cancellationToken))
            {
                throw new AccessDeniedException();
            }
        }
    }
}