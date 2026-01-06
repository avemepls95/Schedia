using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Запрос на удаление сущности.
/// </summary>
[DebuggerDisplay("{Ids}")]
public class DeleteCommand<TEntity> : IRequest, ICqrsOperation
    where TEntity : class
{
    public Id<TEntity>[] Ids { get; set; }

    protected DeleteCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к удалению</param>
    public DeleteCommand(Id<TEntity> id)
    {
        Ids = [id];
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к удалению</param>
    public DeleteCommand(Id<TEntity>[] ids)
    {
        Ids = ids;
    }

    public string Name => $"Delete{typeof(TEntity).Name}.Command";
}