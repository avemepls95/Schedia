namespace Avemepls.Infrastructure.DateTime;

/// <summary>
/// Abstraction to provide current date and time. Useful for testing purposes
/// </summary>
public interface ICurrentDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}