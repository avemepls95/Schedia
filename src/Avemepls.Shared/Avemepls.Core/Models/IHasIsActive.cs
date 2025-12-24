namespace Avemepls.Core.Models;

public interface IHasIsActive
{
    bool IsActive { get; set; }
}

public interface IHasIsActiveNullable
{
    bool? IsActive { get; set; }
}