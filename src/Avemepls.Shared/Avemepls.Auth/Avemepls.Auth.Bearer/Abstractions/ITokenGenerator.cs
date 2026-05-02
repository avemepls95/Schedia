namespace Avemepls.Auth.Bearer.Abstractions;

public interface ITokenGenerator
{
    TokenInformation Create<TId>(UserData<TId> userData)
        where TId : struct;

    TokenInformation Refresh<TId>(string refreshToken, UserData<TId> userData)
        where TId : struct;
}