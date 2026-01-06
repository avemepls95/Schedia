namespace Avemepls.Domain.Commands;

/// <summary>
/// Command that affects some entity of type TEntity and with specific Id
/// </summary>
public interface IAffectEntityCommand : IAffectEntityCommand<int>
{
}

/// <summary>
/// Command that affects some entity of type TEntity and with specific Id
/// </summary>
/// <typeparam name="TId">Type of Entity identifier</typeparam>
public interface IAffectEntityCommand<out TId>
{
    /// <summary>
    /// Entity identifier
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Entity type name
    /// </summary>
    public Type EntityType { get; }
}