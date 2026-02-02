namespace Avemepls.Core.Models;

public interface IHasId : IHasId<int>
{
}

public interface IHasId<T>
{
    T Id { get; set; }
}