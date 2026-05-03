namespace Avemepls.Infrastructure.RateLimit;

/// <summary>
/// Thresholds applied by an <see cref="IRateLimiter"/>: minimum interval between
/// consecutive operations and rolling hourly/daily caps. A value of zero disables
/// the corresponding check.
/// </summary>
public class RateLimitPolicy
{
    public int MinIntervalSeconds { get; set; } = 60;

    public int MaxPerHour { get; set; } = 5;

    public int MaxPerDay { get; set; } = 10;
}