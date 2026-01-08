namespace Avemepls.Infrastructure.DateTime;

public class CurrentSystemDateTimeProvider : ICurrentDateTimeProvider
{
    public DateTimeOffset Now { get; }
}