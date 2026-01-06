namespace Avemepls.Core.Models;

public interface IHasDateModified
{
    DateTimeOffset DateModified { get; set; }
}