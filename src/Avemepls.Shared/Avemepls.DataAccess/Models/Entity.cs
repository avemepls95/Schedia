using System.ComponentModel.DataAnnotations;

using Avemepls.Core.Models;

namespace Avemepls.Core.DataAccess.Models;

/// <summary>
/// Base class for database entities
/// </summary>
public abstract class Entity : Entity<int>, IHasId
{
}

#pragma warning disable SA1402
public abstract class Entity<T> : IHasId<T>
#pragma warning restore SA1402
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public T Id { get; set; }
}