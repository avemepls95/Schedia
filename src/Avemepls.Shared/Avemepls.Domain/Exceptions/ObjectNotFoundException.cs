using Avemepls.Core.Models;

namespace Avemepls.Domain.Exceptions;

public class ObjectNotFoundException : ObjectNotFoundException<int>
{
    public ObjectNotFoundException(Type objectType, string reason) : base(objectType, reason)
    {
    }

    public ObjectNotFoundException(Type objectType, int objectId) : base(objectType, objectId)
    {
    }

    public ObjectNotFoundException(Exception innerException, Type objectType, int objectId) : base(innerException, objectType, objectId)
    {
    }
}

#pragma warning disable SA1402
public class ObjectNotFoundException<TEntity, TId> : ObjectNotFoundException<TId>
#pragma warning restore SA1402
{
    public ObjectNotFoundException(string reason) : base(typeof(TEntity), reason)
    {
    }

    public ObjectNotFoundException(TId objectId) : base(typeof(TEntity), objectId)
    {
    }

    public ObjectNotFoundException(Exception innerException, TId objectId) : base(innerException, typeof(TEntity), objectId)
    {
    }
}

#pragma warning disable SA1402
public class ObjectNotFoundException<TId> : Exception
#pragma warning restore SA1402
{
    public Type ObjectType { get; }
    public TId? ObjectId { get; }

    public ObjectNotFoundException(Type objectType, string reason) : base($"Object of type '{objectType.Name}' was not found: {reason}")
    {
    }

    public ObjectNotFoundException(Type objectType, TId? objectId) : base($"Object of type '{objectType.Name}' with id={objectId} was not found")
    {
        ObjectType = objectType;
        ObjectId = objectId;
    }

    public ObjectNotFoundException(Exception innerException, Type objectType, TId? objectId) : base($"Object of type '{objectType.Name}' with id={objectId} was not found", innerException)
    {
        ObjectType = objectType;
        ObjectId = objectId;
    }
}