using System.ComponentModel;
using System.Diagnostics;

using Avemepls.Core.Localization;
using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Queries;

/// <summary>
/// Get entity model by id
/// </summary>
/// <typeparam name="T">Type of model</typeparam>
[DebuggerDisplay("{Id}")]
#pragma warning disable SA1402
public abstract class GetEntityByIdQuery<T>(Id<T> id, bool includeDeleted = true, bool includeNonActive = true)
    : IGetByIdQuery<T>, IRequest<T>, IHasId<T>
    where T : class
#pragma warning restore SA1402
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    [DisplayNameLoc("Идентификатор сущности")]
    public Id<T> Id { get; set; } = id;

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