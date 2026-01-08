namespace Avemepls.Auth.Bearer;

public class TokenInformation
{
    public string AccessToken { get; set; }

    public DateTimeOffset ExpiresIn { get; set; }

    public string RefreshToken { get; set; }
}