using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

/// <summary>
/// <inheritdoc />
/// </summary>
public abstract class DeleteCommand : DeleteCommand<int>
{
    protected DeleteCommand() : base() // Required for proper deserialization
    {
    }

    protected DeleteCommand(int id) : base(id)
    {
    }
}

/// <summary>
/// Запрос на удаление сущности.
/// </summary>
[DebuggerDisplay("{Ids}")]
#pragma warning disable SA1402
public abstract class DeleteCommand<TId> : IRequest
#pragma warning restore SA1402
{
    public TId[] Ids { get; set; }

    protected DeleteCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к удалению</param>
    protected DeleteCommand(TId id)
    {
        Ids = new[] { id };
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к удалению</param>
    protected DeleteCommand(TId[] ids)
    {
        Ids = ids;
    }
}

/// <summary>
/// Запрос на удаление сущности.
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <typeparam name="TId">Тип идентификатора сущности</typeparam>
#pragma warning disable SA1402
public class DeleteCommand<TEntity, TId> : IRequest, ICqrsOperation
#pragma warning restore SA1402
{
    public TId[] Ids { get; set; }

    protected DeleteCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к удалению</param>
    public DeleteCommand(TId id)
    {
        Ids = new[] { id };
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к удалению</param>
    public DeleteCommand(TId[] ids)
    {
        Ids = ids;
    }

    public string Name => $"Delete{typeof(TEntity).Name}.Command";
}