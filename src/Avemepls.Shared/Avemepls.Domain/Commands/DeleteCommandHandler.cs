using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Security;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Security.Principal;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Commands;

/// <inheritdoc />
#pragma warning disable SA1402
public abstract class DeleteCommandHandler<TContext, TEntity>(
    TContext context,
    IEnumerable<IPermissionEvaluator<TEntity>>? evaluators,
    ICurrentDateTimeProvider currentDateTimeProvider,
    IPrincipalAccessor? principal = null)
    : DeleteCommandHandler<DeleteCommand<TEntity>,
#pragma warning restore SA1402
        TContext, TEntity>(context, evaluators, currentDateTimeProvider, principal)
    where TEntity : class, IHasId<TEntity>, new()
    where TContext : DbContext
{
}

/// <summary>
/// Удаляет запись без её загрузки из базы данных, если не заданы валидаторы (evaluators)
/// </summary>
#pragma warning disable SA1402
public abstract class DeleteCommandHandler<TCommand, TContext, TEntity> : IRequestHandler<TCommand>
#pragma warning restore SA1402
    where TCommand : DeleteCommand<TEntity>, IRequest
    where TEntity : class, IHasId<TEntity>, new()
    where TContext : DbContext
{
#pragma warning disable CA1051
    private readonly IEnumerable<IPermissionEvaluator<TEntity>>? _evaluators;
    private readonly ICurrentDateTimeProvider _currentDateTimeProvider;
    private readonly IPrincipalAccessor? _principalAccessor;
#pragma warning restore CA1051

    protected TContext Context { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DeleteCommandHandler{TCommand, TContext, TEntity}"/>.
    /// </summary>
    protected DeleteCommandHandler(
        TContext context,
        IEnumerable<IPermissionEvaluator<TEntity>>? evaluators,
        ICurrentDateTimeProvider currentDateTimeProvider,
        IPrincipalAccessor? principal = null)
    {
        Context = context;
        _evaluators = evaluators;
        _currentDateTimeProvider = currentDateTimeProvider;
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
            if (request.Ids.Length == 1)
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
                hasDateDeleted.DateDeleted ??= _currentDateTimeProvider.Now;

                if (hasDateDeleted is IHasUserDeleted hasUserDeleted)
                {
                    if (_principalAccessor == null)
                    {
                        throw new InvalidOperationException("Can't process deleted user because no PrincipalAccessor instance specified");
                    }

                    var principal = await _principalAccessor.GetPrincipal();
                    hasUserDeleted.UserDeletedId = principal.GetId();
                }
            }
            else
            {
                Context.Entry(entity).State = EntityState.Deleted;
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

                if (!await evaluator.CanDelete(entity, cancellationToken))
                {
                    throw new AccessDeniedException();
                }
            }
        }
    }
}