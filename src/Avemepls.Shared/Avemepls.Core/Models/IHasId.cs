namespace Avemepls.Core.Models;

public interface IHasId<out TId, TType>
    where TId : IHasValue<TType>
{
    public TId Id { get; }
}