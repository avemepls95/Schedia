using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Avemepls.Auth.Bearer.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Avemepls.Auth.Bearer;

internal sealed class TokenGenerator(OAuthOptions authOptions, IMemoryCache cachingService) : ITokenGenerator
{
    public TokenInformation Create<T>(T id)
        where T : struct
    {
        var claims = GetClaims(id);

        var credentials = new SigningCredentials(
            authOptions.GetSymmetricSecurityKey(),
            SecurityAlgorithms.HmacSha256);

        var expiresIn = DateTimeOffset.Now.AddMinutes(authOptions.AccessTokenLifetimeInMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresIn.DateTime,
            SigningCredentials = credentials,
            Issuer = authOptions.Issuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = GenerateRefreshToken();

        cachingService.Set(id.ToString(), refreshToken);

        var result = new TokenInformation
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken
        };

        return result;
    }

    public TokenInformation Refresh<T>(string refreshToken, T id)
        where T : struct
    {
        EnsureRefreshTokenIsValidAsync(id.ToString()!, refreshToken);

        var newTokenInformation = Create(id);
        return newTokenInformation;
    }

    private void EnsureRefreshTokenIsValidAsync(string idAsString, string refreshToken)
    {
        var storedRefreshToken = cachingService.Get<string>(idAsString);

        if (storedRefreshToken is null || !storedRefreshToken.Equals(refreshToken, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Specified refresh token is not valid.");
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private static Claim[] GetClaims<T>(T id)
        where T : struct
    {
        var claims = new Claim[]
        {
            new("Id", id.ToString() ?? string.Empty)
        };

        return claims;
    }
}