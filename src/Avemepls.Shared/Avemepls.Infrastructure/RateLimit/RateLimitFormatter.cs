namespace Avemepls.Infrastructure.RateLimit;

/// <summary>
/// Helpers for shaping <see cref="IRateLimiter"/> retry-after values into
/// values suitable for end-user messages.
/// </summary>
public static class RateLimitFormatter
{
    /// <summary>
    /// Rounds a retry-after duration up to whole minutes, with a minimum of one,
    /// for display in user-facing messages.
    /// </summary>
    public static int ToUserFriendlyMinutes(TimeSpan retryAfter)
    {
        var minutes = (int)Math.Ceiling(retryAfter.TotalMinutes);
        return minutes < 1 ? 1 : minutes;
    }
}