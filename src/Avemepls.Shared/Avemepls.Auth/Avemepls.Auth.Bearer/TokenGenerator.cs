using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Core.Extensions;
using Avemepls.Security.Principal;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Avemepls.Auth.Bearer;

internal sealed class TokenGenerator(OAuthOptions authOptions, IMemoryCache cachingService) : ITokenGenerator
{
    public TokenInformation Create<T>(UserData<T> userData)
        where T : struct
    {
        var claims = GetClaims(userData);

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

        cachingService.Set(userData.Id.ToString(), refreshToken);

        var result = new TokenInformation
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken
        };

        return result;
    }

    public TokenInformation Refresh<T>(string refreshToken, UserData<T> userData)
        where T : struct
    {
        EnsureRefreshTokenIsValidAsync(userData.Id.ToString()!, refreshToken);

        var newTokenInformation = Create(userData);
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

    private static IEnumerable<Claim> GetClaims<T>(UserData<T> userData)
        where T : struct
    {
        yield return new Claim(Constants.ClaimTypes.UserId, userData.Id.ToString());

        if (!userData.UserName.IsNullOrWhiteSpace())
        {
            yield return new Claim(Constants.ClaimTypes.Login, userData.UserName);
        }

        if (!userData.FullName.IsNullOrWhiteSpace())
        {
            yield return new Claim(Constants.ClaimTypes.FullName, userData.FullName!);
        }

        if (!userData.Email.IsNullOrWhiteSpace())
        {
            yield return new Claim(ClaimTypes.Email, userData.Email!);
        }
    }
}