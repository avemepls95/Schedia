namespace Avemepls.Core.Models;

public interface IHasDateCreated
{
    DateTimeOffset DateCreated { get; set; }
}