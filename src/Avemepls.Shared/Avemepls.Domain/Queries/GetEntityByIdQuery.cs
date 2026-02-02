using System.Diagnostics;

using Avemepls.Core.Localization;
using Avemepls.Core.Models;

using MediatR;
namespace Avemepls.Domain.Queries;

public abstract class GetEntityByIdQuery<T>(int id, bool includeDeleted = true, bool includeNonActive = true)
    : GetEntityByIdQuery<T, int>(id, includeDeleted, includeNonActive);

/// <summary>
/// Get entity model by id
/// </summary>
/// <typeparam name="T">Type of model</typeparam>
/// <typeparam name="TId">Type of identifier</typeparam>
[DebuggerDisplay("{Id}")]
#pragma warning disable SA1402
public abstract class GetEntityByIdQuery<T, TId>(TId id, bool includeDeleted = true, bool includeNonActive = true)
    : IGetByIdQuery<TId>, IRequest<T>, IHasId<TId>
#pragma warning restore SA1402
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    [DisplayNameLoc("Идентификатор сущности")]
    public TId Id { get; set; } = id;

    /// <summary>
    /// Искать ли среди удаленных.
    /// </summary>
    public bool IncludeDeleted { get; set; } = includeDeleted;

    /// <summary>
    /// Искать ли среди неактивных.
    /// </summary>
    public bool IncludeNonActive { get; set; } = includeNonActive;

    /// <summary>
    /// Определяет, следует ли игнорировать вообще все модификаторы при выборке
    /// </summary>
    internal bool IgnoreAllQueryableModifiers { get; set; }
}