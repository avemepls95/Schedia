namespace Avemepls.Infrastructure.RateLimit;

/// <summary>
/// Throttles operations identified by a key according to a configurable policy.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to register a new operation against the given key and returns whether
    /// it is allowed. When denied, the result carries the time the caller should wait
    /// before retrying.
    /// </summary>
    Task<RateLimitResult> Acquire(string key, RateLimitPolicy policy, CancellationToken cancellationToken = default);
}