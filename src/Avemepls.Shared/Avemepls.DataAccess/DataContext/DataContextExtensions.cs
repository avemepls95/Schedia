using Avemepls.Core.Models;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Core.DataAccess.DataContext;

public static class DataContextExtensions
{
    /// <summary>
    /// Returns first entity matched by code
    /// </summary>
    public static async Task<T?> GetByCodeAsync<T>(this IQueryable<T> queryable, string code, CancellationToken cancellationToken)
        where T : IHasCode
    {
        return await queryable.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    /// <summary>
    /// Returns first entity matched by code
    /// </summary>
    public static T? GetByCode<T>(this IQueryable<T> queryable, string code)
        where T : IHasCode
    {
        return queryable.FirstOrDefault(e => e.Code == code);
    }
}