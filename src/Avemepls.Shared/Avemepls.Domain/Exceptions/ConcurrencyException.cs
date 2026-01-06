namespace Avemepls.Domain.Exceptions;

public class ConcurrencyException(Type objectType, int entityId) : ConcurrencyException<int>(objectType, entityId);

#pragma warning disable SA1402
public class ConcurrencyException<TId> : Exception
#pragma warning restore SA1402
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ConcurrencyException&lt;T&gt;"/>.
    /// </summary>
    public ConcurrencyException(Type objectType, TId entityId)
        : base($"Entity '{objectType}' with Id={entityId} already changed")
    {
        ObjectType = objectType;
        EntityId = entityId;
    }

    public Type ObjectType { get; set; }

    public TId EntityId { get; set; }
}