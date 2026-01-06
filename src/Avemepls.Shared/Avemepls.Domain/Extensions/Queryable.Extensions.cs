using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Filters;
using Avemepls.Domain.Queries;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Domain.Extensions;

public static class QueryableExtensions
{
    public static async Task<IQueryable<TEntity>> ApplyModifiers<TEntity>(
        this IQueryable<TEntity> query,
        IEnumerable<IQueryableModifier<TEntity>>? modifiers,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (modifiers == null) return query;

        foreach (var modifier in modifiers)
            query = await modifier.Modify(query, cancellationToken);

        return query;
    }

    public static async Task<IQueryable<TEntity>> ApplyModifiers<TEntity, TModel, TRequest, TResult>(
        this IQueryable<TEntity> query,
        IEnumerable<IQueryableModifier<TEntity>>? modifiers,
        TRequest? request,
        CancellationToken cancellationToken)
        where TEntity : class
        where TModel : class
        where TResult : PagedResponse<TModel>, new()
        where TRequest : ListQuery<TModel, TResult>
    {
        if (modifiers == null || request?.IgnoreAllQueryableModifiers == true) return query;

        var availModifiers = modifiers.Where(m => request?.IgnoreQueryableModifiers == null ||
                                                  !request.IgnoreQueryableModifiers.Contains(m.GetType()));

        foreach (var modifier in availModifiers)
            query = await modifier.Modify(query, cancellationToken);

        return query;
    }

    public static async ValueTask<TEntity> GetByIdAsync<TEntity>(
        this IQueryable<TEntity> entities,
        Id<TEntity> id,
        CancellationToken cancellationToken)
        where TEntity : class, IHasId<TEntity>
    {
        return await entities
                   .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
               throw new ObjectNotFoundException<TEntity>(id);
    }
}