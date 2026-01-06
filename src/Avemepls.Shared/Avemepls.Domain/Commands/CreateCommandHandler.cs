using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Security;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Creates entity
/// </summary>
public abstract class CreateCommandHandler<TCommand, TContext, TEntity>(
    TContext context,
    IMapper mapper,
    IEnumerable<IPermissionEvaluator<TEntity>> evaluators)
    : IRequestHandler<TCommand, Id<TEntity>>
    where TCommand : ICreateCommand, IRequest<Id<TEntity>>
    where TContext : DbContext
    where TEntity : class, IHasId<TEntity>, new()
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
    public virtual async Task<Id<TEntity>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await TryGet(request, cancellationToken);

        if (entity is not null)
        {
            throw new ObjectExistsException<TEntity>(entity.Id);
        }

        entity = await CreateEntity(request, cancellationToken);
        Context.Set<TEntity>().Add(entity);
        await ValidateInsertAccess(entity, cancellationToken);

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
        return ValueTask.FromResult(Mapper.Map<TEntity>(request));
    }

    /// <summary>
    /// Fires after entity changes saved to DB
    /// </summary>
    protected virtual Task EntitySaved(TCommand request, TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Try to load entity from DB
    /// </summary>
    protected virtual Task<TEntity?> TryGet(TCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult<TEntity?>(null);
    }

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
}