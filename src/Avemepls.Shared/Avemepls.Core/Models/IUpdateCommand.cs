namespace Avemepls.Core.Models;

/// <summary>
/// Command to update entity
/// </summary>
public interface IUpdateCommand<TId>
{
    public TId Id { get; set; }
}