using Avemepls.Core.Models;

namespace Avemepls.Domain.Exceptions;

public class ObjectExistsException<TEntity> : Exception
    where TEntity : class
{
    public Id<TEntity>? ObjectId { get; }

    public ObjectExistsException(string reason) : base(
        $"Object of type '{typeof(TEntity)}' was not found: {reason}")
    {
    }

    public ObjectExistsException(Id<TEntity>? objectId) : base(
        $"Object {objectId} was not found")
    {
        ObjectId = objectId;
    }

    public ObjectExistsException(Exception innerException, Id<TEntity>? objectId) : base(
        $"Object {objectId} was not found",
        innerException)
    {
        ObjectId = objectId;
    }
}