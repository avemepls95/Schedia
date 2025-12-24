namespace Avemepls.Core.Models;

public interface IHasDateDeleted
{
    DateTime? DateDeleted { get; set; }

    bool IsDeleted => DateDeleted.HasValue;
}