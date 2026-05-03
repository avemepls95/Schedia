using System.Text.Json;

using Avemepls.Infrastructure.DateTime;

using Microsoft.Extensions.Caching.Distributed;

namespace Avemepls.Infrastructure.RateLimit;

/// <summary>
/// <see cref="IRateLimiter"/> backed by an <see cref="IDistributedCache"/>. Persists
/// attempt timestamps as JSON per key and prunes them on every call to evaluate
/// cooldown, hourly and daily limits.
/// </summary>
internal sealed class DistributedCacheRateLimiter(
    IDistributedCache cache,
    ICurrentDateTimeProvider currentDateTimeProvider) : IRateLimiter
{
    private const string KeyPrefix = "rl:";
    private static readonly TimeSpan StateTtl = TimeSpan.FromHours(25);

    /// <inheritdoc />
    public async Task<RateLimitResult> Acquire(
        string key,
        RateLimitPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var now = currentDateTimeProvider.UtcNow;
        var cacheKey = KeyPrefix + key;

        var raw = await cache.GetStringAsync(cacheKey, cancellationToken);
        var attempts = raw is null
            ? []
            : (JsonSerializer.Deserialize<List<DateTimeOffset>>(raw) ?? []);

        var dayStart = now - TimeSpan.FromDays(1);
        attempts = attempts.Where(a => a >= dayStart).ToList();

        if (policy.MinIntervalSeconds > 0 && attempts.Count > 0)
        {
            var last = attempts.Max();
            var sinceLast = now - last;
            var cooldown = TimeSpan.FromSeconds(policy.MinIntervalSeconds);
            if (sinceLast < cooldown)
            {
                return RateLimitResult.Throttled(cooldown - sinceLast);
            }
        }

        if (policy.MaxPerHour > 0)
        {
            var hourStart = now - TimeSpan.FromHours(1);
            var inHour = attempts.Where(a => a >= hourStart).ToList();
            if (inHour.Count >= policy.MaxPerHour)
            {
                return RateLimitResult.Throttled(inHour.Min() + TimeSpan.FromHours(1) - now);
            }
        }

        if (policy.MaxPerDay > 0 && attempts.Count >= policy.MaxPerDay)
        {
            return RateLimitResult.Throttled(attempts.Min() + TimeSpan.FromDays(1) - now);
        }

        attempts.Add(now);
        var json = JsonSerializer.Serialize(attempts);
        await cache.SetStringAsync(
            cacheKey,
            json,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = StateTtl },
            cancellationToken);

        return RateLimitResult.Success();
    }
}