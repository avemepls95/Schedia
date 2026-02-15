namespace Avemepls.Infrastructure.DateTime;

public class CurrentSystemDateTimeProvider : ICurrentDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}