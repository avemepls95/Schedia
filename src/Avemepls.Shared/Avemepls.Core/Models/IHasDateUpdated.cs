namespace Avemepls.Core.Models;

public interface IHasDateUpdated
{
    DateTimeOffset? DateUpdated { get; set; }
}