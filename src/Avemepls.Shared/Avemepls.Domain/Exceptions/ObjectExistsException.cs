namespace Avemepls.Domain.Exceptions;

public class ObjectExistsException : ObjectExistsException<int>
{
    public ObjectExistsException(Type objectType, string reason) : base(objectType, reason)
    {
    }

    public ObjectExistsException(Type objectType, int objectId) : base(objectType, objectId)
    {
    }

    public ObjectExistsException(Exception innerException, Type objectType, int objectId) : base(
        innerException,
        objectType,
        objectId)
    {
    }
}

#pragma warning disable SA1402
public class ObjectExistsException<TId> : Exception
#pragma warning restore SA1402
{
    public Type ObjectType { get; }

    public TId? ObjectId { get; }

    public ObjectExistsException(Type objectType, string reason) : base(
        $"Object of type '{objectType.Name}' was not found: {reason}")
    {
    }

    public ObjectExistsException(Type objectType, TId? objectId) : base(
        $"Object of type '{objectType.Name}' with id={objectId} was not found")
    {
        ObjectType = objectType;
        ObjectId = objectId;
    }

    public ObjectExistsException(Exception innerException, Type objectType, TId? objectId) : base(
        $"Object of type '{objectType.Name}' with id={objectId} was not found",
        innerException)
    {
        ObjectType = objectType;
        ObjectId = objectId;
    }
}

#pragma warning disable SA1402
public class ObjectExistsException<TEntity, TId> : ObjectExistsException<TId>
#pragma warning restore SA1402
{
    public ObjectExistsException(string reason) : base(typeof(TEntity), reason)
    {
    }

    public ObjectExistsException(TId objectId) : base(typeof(TEntity), objectId)
    {
    }

    public ObjectExistsException(Exception innerException, TId objectId) : base(innerException, typeof(TEntity), objectId)
    {
    }
}