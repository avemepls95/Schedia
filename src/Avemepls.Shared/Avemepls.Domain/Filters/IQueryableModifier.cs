namespace Avemepls.Domain.Filters;

/// <summary>
/// Additional filter to modify entities queries. Allows to construct some "restrictors" depending on context (user, environment, tenancy etc)
/// </summary>
public interface IQueryableModifier<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Applies additional expressions to queryable
    /// </summary>
    /// <param name="query">Query to modify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IQueryable<TEntity>> Modify(IQueryable<TEntity> query, CancellationToken cancellationToken);
}