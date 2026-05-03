using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.RateLimit;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Avemepls.Infrastructure.Tests.RateLimit;

public class DistributedCacheRateLimiterTests
{
    private readonly DistributedCacheRateLimiter _sut;
    private DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public DistributedCacheRateLimiterTests()
    {
        IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var dateTimeProvider = Substitute.For<ICurrentDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(_ => _now);
        _sut = new DistributedCacheRateLimiter(cache, dateTimeProvider);
    }

    [Fact]
    public async Task Acquire_Should_Allow_When_NoPriorAttempts()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 60, MaxPerHour = 5, MaxPerDay = 10 };

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
        result.RetryAfter.Should().BeNull();
    }

    [Fact]
    public async Task Acquire_Should_Throttle_When_WithinCooldownInterval()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 60, MaxPerHour = 5, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromSeconds(30));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RetryAfter.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task Acquire_Should_Allow_When_CooldownElapsed()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 60, MaxPerHour = 5, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromSeconds(61));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_NotApplyCooldown_When_MinIntervalIsZero()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 5, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_Throttle_When_HourlyLimitExceeded()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 3, MaxPerDay = 100 };
        for (var i = 0; i < 3; i++)
        {
            await _sut.Acquire("user-1", policy);
            Advance(TimeSpan.FromMinutes(1));
        }

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RetryAfter.Should().NotBeNull();
        result.RetryAfter!.Value.Should().BeGreaterThan(TimeSpan.Zero);
        result.RetryAfter!.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task Acquire_Should_Allow_When_HourlyWindowSlidesOut()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 2, MaxPerDay = 100 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromMinutes(10));
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromMinutes(55));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_Throttle_When_DailyLimitExceeded()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 100, MaxPerDay = 4 };
        for (var i = 0; i < 4; i++)
        {
            await _sut.Acquire("user-1", policy);
            Advance(TimeSpan.FromHours(2));
        }

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task Acquire_Should_Allow_When_DailyWindowSlidesOut()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 100, MaxPerDay = 2 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromHours(2));
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromHours(23));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_TrackKeysIndependently()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 60, MaxPerHour = 1, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);

        // Act
        var result = await _sut.Acquire("user-2", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_PersistStateAcrossCalls()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 2, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);
        await _sut.Acquire("user-1", policy);

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task Acquire_Should_ReturnCooldownRetryAfter_When_BothCooldownAndHourlyExceeded()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 60, MaxPerHour = 1, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromSeconds(10));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RetryAfter.Should().Be(TimeSpan.FromSeconds(50));
    }

    [Fact]
    public async Task Acquire_Should_PruneAttemptsOlderThanDayWindow()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 100, MaxPerDay = 3 };
        for (var i = 0; i < 3; i++)
        {
            await _sut.Acquire("user-1", policy);
            Advance(TimeSpan.FromHours(2));
        }

        Advance(TimeSpan.FromHours(20));

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Acquire_Should_ReportRetryAfterUntilOldestHourlyAttemptExpires()
    {
        // Arrange
        var policy = new RateLimitPolicy { MinIntervalSeconds = 0, MaxPerHour = 2, MaxPerDay = 10 };
        await _sut.Acquire("user-1", policy);
        Advance(TimeSpan.FromMinutes(20));
        await _sut.Acquire("user-1", policy);

        // Act
        var result = await _sut.Acquire("user-1", policy);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RetryAfter.Should().Be(TimeSpan.FromMinutes(40));
    }

    private void Advance(TimeSpan delta) => _now = _now.Add(delta);
}
