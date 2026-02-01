namespace Avemepls.Core.Models;

// ReSharper disable once UnusedTypeParameter
#pragma warning disable S2326
public interface ICrossDomainRequest<TResponse> : ICrossDomainRequestBase
    where TResponse : class, new()
{
}

public interface ICrossDomainCommand : ICrossDomainRequestBase
{
}
#pragma warning restore S2326