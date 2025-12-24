namespace Avemepls.Core.Models;

public interface IHasUserDeleted : IHasUserDeleted<int>
{
}

public interface IHasUserDeleted<TId>
    where TId : struct
{
    TId? UserDeletedId { get; set; }
}