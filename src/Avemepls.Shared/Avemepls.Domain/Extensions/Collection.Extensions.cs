using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;

namespace Avemepls.Domain.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Synchronize two collections and make target containing the same elements as source.
    /// Extra elements in target collection will be deleted. Missing elements will be inserted. Existing will be updated via mapper method.
    /// </summary>
    /// <param name="source">Source collection</param>
    /// <param name="target">Target collection</param>
    /// <param name="mapper">Function to map source item to target</param>
    /// <param name="deleter">Optional action, will be called for each deleted item in target collection.</param>
    public static (int Added, int Updated, int Deleted) Sync<TSource, TTarget>(
        this ICollection<TSource>? source,
        IList<TTarget> target,
        Func<TSource, TTarget, TTarget> mapper,
        Action<TTarget>? deleter = null)
        where TTarget : class, IHasId, new()
        where TSource : IHasId
    {
        return source.Sync<TSource, TTarget, int>(target, mapper, deleter);
    }

    /// <summary>
    /// Synchronize two collections and make target containing the same elements as source.
    /// Extra elements in target collection will be deleted. Missing elements will be inserted. Existing will be updated via mapper method.
    /// </summary>
    /// <param name="source">Source collection</param>
    /// <param name="target">Target collection</param>
    /// <param name="mapper">Function to map source item to target</param>
    /// <param name="deleter">Optional action, will be called for each deleted item in target collection.</param>
    public static (int Added, int Updated, int Deleted) Sync<TSource, TTarget, TId>(
        this ICollection<TSource>? source,
        IList<TTarget> target,
        Func<TSource, TTarget, TTarget> mapper,
        Action<TTarget>? deleter = null)
        where TTarget : class, IHasId<TId>, new()
        where TSource : IHasId<TId>
        where TId : IComparable<TId>
    {
        var (inserted, updated, deleted) = (0, 0, 0);

        if (source != null)
        {
            var index = 0;

            // Update existing items or insert new
            foreach (var src in source.OrderByDescending(x => x.Id))
            {
                if (index >= target.Count)
                {
                    var created = mapper(src, new TTarget());
                    created.Id = default!;
                    target.Add(created);
                    inserted++;
                }
                else
                {
                    var id = target[index].Id;
                    target[index] = mapper(src, target[index]);
                    target[index].Id = id;
                    updated++;
                }

                index++;
            }

            var targetCount = target.Count;

            // Remove tail
            for (var i = index; i < targetCount; i++)
            {
                deleter?.Invoke(target[i]);
                target.RemoveAt(index);
                deleted++;
            }
        }

        return (inserted, updated, deleted);
    }

    /// <summary>
    /// Synchronize two collections and make target containing the same elements (by Id) as source in the original order
    /// </summary>
    /// <param name="source">Source collection</param>
    /// <param name="target">Target collection</param>
    /// <param name="mapExisting">Function to map source item to target</param>
    /// <param name="deleter">Optional action, will be called for each deleted item in target collection.</param>
    public static (int Added, int Updated, int Deleted) SyncByCompare<TSource, TTarget>(
        this ICollection<TSource>? source,
        IList<TTarget> target,
        Func<TSource, TTarget, TTarget> mapExisting,
        Action<TTarget>? deleter = null)
        where TTarget : IHasId, new()
        where TSource : IHasId
    {
        return source.SyncByCompare(target,
                                    mapExisting,
                                    (s, t) => s.Id != 0 && s.Id == t.Id,
                                    deleter);
    }

    /// <summary>
    /// Synchronize two collections and make target containing the same elements as source in the original order
    /// It requires to specify comparer for "same" elements criteria in collections.
    /// </summary>
    /// <param name="source">Source collection</param>
    /// <param name="target">Target collection</param>
    /// <param name="mapper">Function to map source item to target</param>
    /// <param name="lookup">Optional comparer for searching same elements in collections.</param>
    /// <param name="deleter">Optional action, will be called for each deleted item in target collection.</param>
    /// <param name="createNewItems">If no item in target collection found, new one will be created.</param>
    public static (int Added, int Updated, int Deleted) SyncByCompare<TSource, TTarget>(
        this ICollection<TSource>? source,
        ICollection<TTarget> target,
        Func<TSource, TTarget, TTarget> mapper,
        Func<TSource, TTarget, bool> lookup,
        Action<TTarget>? deleter = null,
        bool createNewItems = true)
        where TTarget : new()
    {
        var (inserted, updated, deleted) = (0, 0, 0);

        if (source != null)
        {
            foreach (var trg in target.Where(x => !source.Any(s => lookup(s, x))))
            {
                deleter?.Invoke(trg);
                target.Remove(trg);
                deleted++;
            }

            foreach (var src in source)
            {
                var trg = target.FirstOrDefault(t => lookup(src, t));

                if (Equals(trg, default(TTarget)))
                {
                    var created = mapper(src, new TTarget());

                    if (createNewItems && created is IHasId hasId)
                    {
                        hasId.Id = 0;
                    }

                    target.Add(created);
                    inserted++;
                }
                else
                {
                    if (trg is IHasId hasId)
                    {
                        var id = hasId.Id;
                        mapper(src, trg);
                        hasId.Id = id;
                    }
                    else
                    {
                        mapper(src, trg!);
                    }

                    updated++;
                }
            }
        }

        return (inserted, updated, deleted);
    }

    public static TEntity GetById<TEntity>(this IEnumerable<TEntity> entities, int id)
        where TEntity : IHasId<int>
    {
        return entities
            .FirstOrDefault(x => x.Id == id) ?? throw new ObjectNotFoundException<TEntity, int>(id);
    }
}