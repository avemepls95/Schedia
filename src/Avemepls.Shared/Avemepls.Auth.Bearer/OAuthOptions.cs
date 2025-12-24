using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Avemepls.Auth.Bearer;

public sealed class OAuthOptions
{
    public string SecretKey { get; set; }

    public string Issuer { get; set; }

    public int AccessTokenLifetimeInMinutes { get; set; }

    public OAuthOptions()
    {
    }

    public SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
    }
}