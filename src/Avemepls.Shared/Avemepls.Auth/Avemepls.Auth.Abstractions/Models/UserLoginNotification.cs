using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Auth.Abstractions.Models;

public class UserLoginNotification : INotification, IAsyncNotification
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }
}