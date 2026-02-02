namespace Avemepls.Core.Models;

public interface IGetByIdQuery<TId>
{
    public TId Id { get; set; }
}