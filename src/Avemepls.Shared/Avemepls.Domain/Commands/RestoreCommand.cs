using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Запрос на восстановление сущности.
/// </summary>
[DebuggerDisplay("{Ids}")]
public class RestoreCommand<TEntity> : IRequest
    where TEntity : class, IHasDateDeleted
{
    public Id<TEntity>[] Ids { get; set; }

    protected RestoreCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к восстановлению</param>
    public RestoreCommand(Id<TEntity> id)
    {
        Ids = [id];
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к восстановлению</param>
    public RestoreCommand(Id<TEntity>[] ids)
    {
        Ids = ids;
    }

    public string Name => $"Restore{typeof(TEntity).Name}.Command";
}