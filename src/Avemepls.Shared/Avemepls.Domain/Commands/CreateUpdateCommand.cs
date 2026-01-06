using System.Diagnostics;

using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Domain.Commands;

/// <summary>
/// Command to create or update entity from model TModel
/// </summary>
[DebuggerDisplay("{Id}: {Model}")]
public abstract class CreateUpdateCommand<TModel> : IRequest<Id<TModel>>
    where TModel : class, IHasId<TModel>
{
    public Id<TModel>? Id { get; }

    public TModel Model { get; }

    protected CreateUpdateCommand() // Required for proper deserialization
    {
    }

    protected CreateUpdateCommand(TModel model) : this(null, model)
    {
    }

    protected CreateUpdateCommand(Id<TModel>? id, TModel model)
    {
        Id = Equals(id, default(Id<TModel>)) ? null : id;
        Model = model;

        if (Id != null && model is IHasId<TModel> modelWithId && !modelWithId.Id.Equals(id))
        {
            modelWithId.Id = Id.Value;
        }
    }
}