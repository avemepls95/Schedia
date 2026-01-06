namespace Avemepls.Infrastructure;

/// <summary>
/// Abstraction to provide current date and time. Useful for testing purposes
/// </summary>
public interface ICurrentDateTimeProvider
{
    DateTimeOffset Now { get; }
}