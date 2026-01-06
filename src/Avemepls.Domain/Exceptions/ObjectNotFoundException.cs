using Avemepls.Core.Models;

namespace Avemepls.Domain.Exceptions;

public class ObjectNotFoundException<TEntity> : Exception
    where TEntity : class
{
    public Id<TEntity>? ObjectId { get; }

    public ObjectNotFoundException(string reason) : base($"Object of type '{typeof(TEntity)}' was not found: {reason}")
    {
    }

    public ObjectNotFoundException(Id<TEntity>? objectId) : base($"Object of type '{typeof(TEntity)}' with id={objectId} was not found")
    {
        ObjectId = objectId;
    }

    public ObjectNotFoundException(Exception innerException, Id<TEntity>? objectId)
        : base($"Object of type '{typeof(TEntity)}' with id={objectId} was not found", innerException)
    {
        ObjectId = objectId;
    }
}