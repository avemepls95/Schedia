namespace Avemepls.Infrastructure.RateLimit;

/// <summary>
/// Outcome of a rate limit check produced by <see cref="IRateLimiter"/>.
/// </summary>
public class RateLimitResult
{
    public bool Allowed { get; init; }

    public TimeSpan? RetryAfter { get; init; }

    /// <summary>
    /// Builds a result indicating the operation is allowed.
    /// </summary>
    public static RateLimitResult Success() => new() { Allowed = true };

    /// <summary>
    /// Builds a result indicating the operation is denied, with the time the caller
    /// should wait before retrying.
    /// </summary>
    public static RateLimitResult Throttled(TimeSpan retryAfter) => new() { Allowed = false, RetryAfter = retryAfter };
}