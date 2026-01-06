using System.ComponentModel;
using System.Net;
using System.Security.Principal;

using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Http;

namespace Avemepls.Infrastructure.Identity;

public class OnlineUser(
    IPrincipalAccessor principalAccessor,
    ICurrentDateTimeProvider currentDateTimeProvider,
    IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Дата и время подключения
    /// </summary>
    [DisplayName("Online since")]
    public System.DateTimeOffset OnlineSince { get; } = currentDateTimeProvider.Now;

    /// <summary>
    /// IP-адрес подключения
    /// </summary>
    [DisplayName("IP address")]
    public IPAddress IpAddress { get; } = httpContextAccessor.HttpContext.Connection.RemoteIpAddress;

    /// <summary>
    /// Тип аутентификации
    /// </summary>
    [DisplayName("Authentication type")]
    public string? AuthenticationType { get; private set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    [DisplayName("Login")]
    public string? Name { get; private set; }

    /// <summary>
    /// Является ли пользователь онлайн?
    /// </summary>
    [DisplayName("Is authenticated")]
    public bool IsAuthenticated { get; private set; }

    private IIdentity? _identity;

    public async Task Initialize()
    {
        _identity ??= (await principalAccessor.GetPrincipal()).Identity ?? new AnonymousIdentity();

        Name = _identity.Name;
        IsAuthenticated = _identity.IsAuthenticated;
        AuthenticationType = _identity.AuthenticationType;
    }
}