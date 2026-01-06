namespace Avemepls.Core.Models;

public interface IHasDateDeleted
{
    DateTimeOffset? DateDeleted { get; set; }

    bool IsDeleted => DateDeleted.HasValue;
}