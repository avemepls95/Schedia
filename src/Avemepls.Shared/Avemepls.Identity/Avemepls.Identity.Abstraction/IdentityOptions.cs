using Avemepls.Infrastructure.RateLimit;

namespace Avemepls.Identity.Abstraction;

public class IdentityOptions
{
    public int EmailConfirmationTokenLifeTimeInHours { get; set; } = 24;

    public RateLimitPolicy EmailConfirmationSendRateLimit { get; set; } = new()
    {
        MinIntervalSeconds = 60,
        MaxPerHour = 5,
        MaxPerDay = 10
    };

    public RateLimitPolicy EmailConfirmationVerifyRateLimit { get; set; } = new()
    {
        MinIntervalSeconds = 0,
        MaxPerHour = 20,
        MaxPerDay = 100
    };
}