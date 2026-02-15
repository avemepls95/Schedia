using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Avemepls.Core.Extensions;
using Avemepls.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Avemepls.Core.DataAccess.Extensions;

/// <summary>
/// Common extensions for IQueryable interface
/// </summary>
public static partial class QueryableExtensions
{
    /// <summary>
    /// Returns single value of some field of some object
    /// </summary>
    /// <param name="queryable">Source queryable</param>
    /// <param name="filter">Filter expression to filter entity</param>
    /// <param name="fieldExtractor">Expression for field value resolver</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="T">Type of returning value</typeparam>
    /// <returns>First value of specified field</returns>
    public static Task<T?> GetFirstValue<TEntity, T>(
        this IQueryable<TEntity> queryable,
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, T>> fieldExtractor,
        CancellationToken cancellationToken)
    {
        return queryable.Where(filter).Select(fieldExtractor).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Returns single value of some field of some object
    /// </summary>
    /// <param name="queryable">Source queryable</param>
    /// <param name="id">Id of entity</param>
    /// <param name="fieldExtractor">Expression for field value resolver</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="T">Type of returning value</typeparam>
    /// <returns>First value of specified field</returns>
    public static Task<T?> GetFirstValue<TEntity, T>(
        this IQueryable<TEntity> queryable,
        int id,
        Expression<Func<TEntity, T>> fieldExtractor,
        CancellationToken cancellationToken)
        where TEntity : IHasId<int>
    {
        return queryable.Where(e => e.Id == id).Select(fieldExtractor).FirstOrDefaultAsync(cancellationToken);
    }

    private static ConcurrentDictionary<Type, MethodInfo> _containMethods = new();

    /// <summary>
    /// Constructs dynamic linq OR expression regarding each key in list
    /// </summary>
    /// <param name="queryable">Queryable</param>
    /// <param name="keySelector">Key selector expression</param>
    /// <param name="identifiers">List of identifiers to match with</param>
    /// <param name="quantityThreshold">Maximum amount of items to join with OR. When exceeded, CONTAINS expression will be used instead.</param>
    /// <param name="executeQueryIfEmpty">Mock query and will not send query to DB if array empty or null</param>
    /// <returns>Queryable with `where` condition applied. All keys are joined with OR</returns>
    public static IQueryable<TSource> WhereIn<TSource, TKey>(
        this IQueryable<TSource> queryable,
        Expression<Func<TSource, TKey>> keySelector,
        TKey[]? identifiers,
        int quantityThreshold = 20,
        bool executeQueryIfEmpty = false)
    {
        if (!executeQueryIfEmpty)
            queryable = queryable.DontExecuteIfEmpty(identifiers);

        if (identifiers is null || identifiers.IsEmpty())
        {
            return queryable;
        }

        // If there are more than 20 identifiers, use Contains
        if (identifiers.Length > quantityThreshold)
        {
            var parameter = keySelector.Parameters.Single();
            var property = keySelector.Body;
            var keyType = typeof(TKey);

            var containsMethod = _containMethods.GetOrAdd(keyType,
                                                          typeof(Enumerable).GetMethods()
                                                              .First(m => m.Name == "Contains" &&
                                                                          m.GetParameters().Length == 2)
                                                              .MakeGenericMethod(keyType));

            var body = Expression.Call(containsMethod,
                                       Expression.Constant(identifiers),
                                       property);

            var predicate = Expression.Lambda<Func<TSource, bool>>(body, parameter);

            return queryable.Where(predicate);
        }

        if (identifiers.Length > 0)
        {
            // Create a parameter expression for the entity type
            var parameter = keySelector.Parameters.Single();

            // Create an empty expression
            Expression? predicate = null;

            foreach (var identifier in identifiers)
            {
                // Create an expression that compares the keySelector with the identifier
                var keySelectorBody = keySelector.Body;
                var equal = Expression.Equal(keySelectorBody, Expression.Constant(identifier));

                predicate = predicate is null
                    ? equal
                    : Expression.OrElse(predicate, equal);
            }

            // Combine the existing query with the new predicate using a lambda expression
            var lambda = Expression.Lambda<Func<TSource, bool>>(predicate!, parameter);

            return queryable.Where(lambda);
        }

        return queryable;
    }

    /// <summary>
    /// Constructs dynamic linq OR expression regarding each key in list
    /// </summary>
    /// <param name="queryable">Queryable</param>
    /// <param name="keySelector">Key selector expression</param>
    /// <param name="identifiers">List of identifiers to match with</param>
    /// <returns>Queryable with `where` condition applied. All keys are joined with OR</returns>
    public static IEnumerable<TSource> WhereIn<TSource, TKey>(
        this IEnumerable<TSource> queryable,
        Expression<Func<TSource, TKey>> keySelector,
        TKey[]? identifiers)
    {
        if (identifiers is null || !identifiers.Any())
        {
            return Array.Empty<TSource>();
        }

        // Original OrWhere method implementation for <= 20 identifiers
        var parameter = keySelector.Parameters.Single();

        var equals = identifiers.Select(value =>
                                            (Expression)Expression.Equal(keySelector.Body,
                                                                         Expression.Constant(value)));

        var body = equals.Aggregate(Expression.OrElse);
        var predicate = Expression.Lambda<Func<TSource, bool>>(body, parameter);

        return queryable.Where(predicate.Compile());
    }

    /// <summary>
    /// Query will not execute if the collection is empty
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="array">Some collection</param>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TArray">Type of item in the collection</typeparam>
    /// <returns>Empty IQueryable if the collection is null or empty, else the source query</returns>
    public static IQueryable<TEntity> DontExecuteIfEmpty<TEntity, TArray>(
        this IQueryable<TEntity> query,
        IEnumerable<TArray>? array)
    {
        if (array is null || !array.Any())
        {
            return new MockAsyncEnumerable<TEntity>(Array.Empty<TEntity>());
        }

        return query;
    }

    private sealed class MockAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _innerQueryProvider;

        internal MockAsyncQueryProvider(IQueryProvider inner)
        {
            _innerQueryProvider = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new MockAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MockAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression) => _innerQueryProvider.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _innerQueryProvider.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            Type expectedResultType = typeof(TResult).GetGenericArguments()[0];
            object? executionResult = this.Execute(expression);

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private sealed class MockAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public MockAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public MockAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider => new MockAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new MockAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private sealed class MockAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public MockAsyncEnumerator(IEnumerator<T> inner)
        {
            _enumerator = inner;
        }

        public T Current => _enumerator.Current;

        public ValueTask DisposeAsync() => new(Task.Run(() => _enumerator.Dispose()));

        public ValueTask<bool> MoveNextAsync() => new(_enumerator.MoveNext());
    }
}