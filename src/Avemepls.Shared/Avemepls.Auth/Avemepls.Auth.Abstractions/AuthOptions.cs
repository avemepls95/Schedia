using Avemepls.Auth.Bearer;

namespace Avemepls.Auth.Abstractions;

public class AuthOptions
{
    public int ResetPasswordTokenLifeTimeInHours { get; set; } = 1;

    public OAuthOptions1 OAuth { get; set; }

    public GoogleAuthOptions? Google { get; set; }
}