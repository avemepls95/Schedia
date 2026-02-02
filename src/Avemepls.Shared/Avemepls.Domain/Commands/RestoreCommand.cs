using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

/// <summary>
/// <inheritdoc />
/// </summary>
public abstract class RestoreCommand : RestoreCommand<int>
{
    protected RestoreCommand() : base() // Required for proper deserialization
    {
    }

    protected RestoreCommand(int id) : base(id)
    {
    }
}

/// <summary>
/// Запрос на восстановление сущности.
/// </summary>
[DebuggerDisplay("{Ids}")]
#pragma warning disable SA1402
public abstract class RestoreCommand<TId> : IRequest
#pragma warning restore SA1402
{
    public TId[] Ids { get; set; }

    protected RestoreCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к восстановлению</param>
    protected RestoreCommand(TId id)
    {
        Ids = new[] { id };
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к восстановлению</param>
    protected RestoreCommand(TId[] ids)
    {
        Ids = ids;
    }
}

/// <summary>
/// Запрос на восстановление сущности.
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <typeparam name="TId">Тип идентификатора сущности</typeparam>
#pragma warning disable SA1402
public class RestoreCommand<TEntity, TId> : IRequest, ICqrsOperation
#pragma warning restore SA1402
    where TEntity : IHasDateDeleted
{
    public TId[] Ids { get; set; }

    protected RestoreCommand() // Required for proper deserialization
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="id">Идентификатор сущностей к восстановлению</param>
    public RestoreCommand(TId id)
    {
        Ids = new[] { id };
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса
    /// </summary>
    /// <param name="ids">Список идентификаторов сущностей к восстановлению</param>
    public RestoreCommand(TId[] ids)
    {
        Ids = ids;
    }

    public string Name => $"Restore{typeof(TEntity).Name}.Command";
}