using Avemepls.Core.Models;

using MediatR;

namespace Avemepls.Auth.Abstractions.Models;

public class UserRegisteredViaPasswordNotification : INotification, IAsyncNotification
{
    public UserRegisteredViaPasswordNotification(int userId)
    {
        UserId = userId;
    }

    public UserRegisteredViaPasswordNotification()
    {
    }

    public int UserId { get; set; }

    public string ConfirmationToken { get; set; }
}