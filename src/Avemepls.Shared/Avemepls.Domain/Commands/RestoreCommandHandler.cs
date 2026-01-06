using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Security;
using Avemepls.Security.Principal;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Восстанавливает запись без её загрузки из базы данных, если не заданы валидаторы (evaluators)
/// </summary>
#pragma warning disable SA1402
public abstract class RestoreCommandHandler<TCommand, TContext, TEntity> : IRequestHandler<TCommand>
#pragma warning restore SA1402
    where TCommand : RestoreCommand<TEntity>, IRequest
    where TEntity : class, IHasId<TEntity>, IHasDateDeleted, new()
    where TContext : DbContext
{
    private readonly IEnumerable<IPermissionEvaluator<TEntity>>? _evaluators;
    private readonly IPrincipalAccessor? _principalAccessor;

    protected TContext Context { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RestoreCommandHandler{TCommand, TContext, TEntity, TId}"/>.
    /// </summary>
    protected RestoreCommandHandler(
        TContext context,
        IEnumerable<IPermissionEvaluator<TEntity>>? evaluators,
        IPrincipalAccessor? principal = null)
    {
        Context = context;
        _evaluators = evaluators;
        _principalAccessor = principal;
    }

    public virtual async Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entities = await Context
            .Set<TEntity>()
            .AsTracking()
            .Where(x => request.Ids.Contains(x.Id))
            .ToArrayAsync(cancellationToken);

        if (request.Ids.Length != entities.Length)
        {
            if (request.Ids.Length == 1 && !entities.Any())
            {
                throw new ObjectNotFoundException<TEntity>(request.Ids[0]);
            }

            throw new ObjectNotFoundException<TEntity>("Object with one of ids not found: " + string.Join(",", request.Ids));
        }

        foreach (var entity in entities)
        {
            await ValidateAccessAsync(entity, cancellationToken);

            if (entity is IHasDateDeleted hasDateDeleted)
            {
                hasDateDeleted.DateDeleted = null;

                if (hasDateDeleted is IHasUserDeleted hasUserDeleted)
                {
                    if (_principalAccessor == null)
                    {
                        throw new InvalidOperationException(
                            "Can't process deleted user because no PrincipalAccessor instance specified");
                    }

                    hasUserDeleted.UserDeletedId = null;
                }
            }
            else
            {
                Context.Entry(entity).State = EntityState.Modified;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateAccessAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (_evaluators != null)
        {
            foreach (var evaluator in _evaluators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await evaluator.CanUpdate(entity, cancellationToken))
                {
                    throw new AccessDeniedException();
                }
            }
        }
    }
}