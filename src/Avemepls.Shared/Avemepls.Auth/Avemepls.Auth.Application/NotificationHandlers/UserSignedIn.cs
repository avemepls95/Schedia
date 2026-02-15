using Avemepls.Auth.Abstractions.Models;
using MassTransit;

namespace Avemepls.Auth.Application.NotificationHandlers;

public class UserSignedIn : IConsumer<UserLoginNotification>
{
    public Task Consume(ConsumeContext<UserLoginNotification> context)
    {
        return Task.CompletedTask;
    }
}