using MassTransit;

using Schedia.Auth.Abstractions.Models;

namespace Schedia.Auth.Application.NotificationHandlers;

public class UserLoginNotificationHandler : IConsumer<UserLoginNotification>
{
    public Task Consume(ConsumeContext<UserLoginNotification> context)
    {
        return Task.CompletedTask;
    }
}