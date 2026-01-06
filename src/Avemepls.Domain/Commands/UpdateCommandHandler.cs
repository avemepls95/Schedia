using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Security;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Updates entity
/// </summary>
#pragma warning disable SA1402
public abstract class UpdateCommandHandler<TCommand, TContext, TEntity>(
    TContext context,
    IMapper mapper,
    IEnumerable<IPermissionEvaluator<TEntity>> evaluators)
    : IRequestHandler<TCommand>
#pragma warning restore SA1402
    where TCommand : IUpdateCommand<TEntity>, IRequest
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
    public virtual async Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await Get(request, cancellationToken);

        await Map(request, entity, cancellationToken);
        await ValidateUpdateAccess(entity, cancellationToken);

        await EntitySaving(request, entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        await EntitySaved(request, entity, cancellationToken);
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
    protected virtual Task Map(TCommand request, TEntity entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(Mapper.Map(request, entity));
    }

    /// <summary>
    /// Try to load entity from DB. Default implementation loads entity by it's primary key Id
    /// </summary>
    protected virtual async Task<TEntity> Get(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await Include(Context.Set<TEntity>())
            .AsTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(request.Id), cancellationToken);

        if (entity == null)
            throw new ObjectNotFoundException<TEntity>(request.Id);

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