namespace Avemepls.Core.Models;

/// <summary>
/// Used to indicate who created entity/record
/// </summary>
public interface IHasUserCreated : IHasUserCreated<int>
{
}

/// <summary>
/// Used to indicate who created entity/record
/// </summary>
public interface IHasUserCreated<TId>
{
    /// <summary>
    /// Identifier of the user
    /// </summary>
    TId UserCreatedId { get; set; }
}