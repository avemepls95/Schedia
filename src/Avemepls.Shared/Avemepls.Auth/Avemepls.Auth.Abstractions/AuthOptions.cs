using Avemepls.Auth.Bearer;
using Avemepls.Infrastructure.RateLimit;

namespace Avemepls.Auth.Abstractions;

public class AuthOptions
{
    public int ResetPasswordTokenLifeTimeInHours { get; set; } = 1;

    public OAuthOptions OAuth { get; set; }

    public GoogleAuthOptions? Google { get; set; }

    public RateLimitPolicy PasswordResetRateLimit { get; set; } = new()
    {
        MinIntervalSeconds = 60,
        MaxPerHour = 5,
        MaxPerDay = 10
    };
}