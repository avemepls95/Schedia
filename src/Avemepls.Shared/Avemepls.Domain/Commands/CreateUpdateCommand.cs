using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

public abstract class CreateUpdateCommand<TModel> : CreateUpdateCommand<int, TModel>
{
    protected CreateUpdateCommand() : base() // Required for proper deserialization
    {
    }

    protected CreateUpdateCommand(TModel model) : base(model)
    {
    }

    protected CreateUpdateCommand(int? id, TModel model) : base(id ?? 0, model)
    {
    }
}

/// <summary>
/// Command to create or update entity from model TModel
/// </summary>
[DebuggerDisplay("{Id}: {Model}")]
#pragma warning disable SA1402
public abstract class CreateUpdateCommand<TId, TModel> : IRequest<TId>
#pragma warning restore SA1402
{
    public TId? Id { get; }

    public TModel Model { get; }

    protected CreateUpdateCommand() // Required for proper deserialization
    {
    }

    protected CreateUpdateCommand(TModel model) : this(default, model)
    {
    }

    protected CreateUpdateCommand(TId? id, TModel model)
    {
        Id = Equals(id, default(TId)) ? default : id;
        Model = model;

#pragma warning disable S2955
        if (Id != null && model is IHasId<TId> modelWithId && modelWithId.Id != null && !modelWithId.Id.Equals(id))
#pragma warning restore S2955
            modelWithId.Id = Id;
    }
}