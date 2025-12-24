namespace Avemepls.Core.Models;

public interface IHasValue<TType>
{
    public TType Value { get; init; }
}