namespace Avemepls.Auth.Bearer.Abstractions;

public interface ITokenGenerator
{
    TokenInformation Create<T>(T id)
        where T : struct;

    TokenInformation Refresh<T>(string refreshToken, T id)
        where T : struct;
}