using Avemepls.Auth.Bearer;

namespace Avemepls.Auth.Abstractions;

public class AuthOptions
{
    public int ResetPasswordTokenLifeTimeInHours { get; set; } = 1;

    public OAuthOptions OAuth { get; set; }

    public GoogleAuthOptions? Google { get; set; }
}