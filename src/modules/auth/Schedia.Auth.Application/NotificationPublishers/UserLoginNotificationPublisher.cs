using Avemepls.ServiceBus.Common;
using Avemepls.ServiceBus.Events;

using MassTransit;

using Schedia.Auth.Abstractions.Models;

namespace Schedia.Auth.Application.NotificationPublishers;

internal sealed class UserLoginNotificationPublisher(IPublishEndpoint publisher, CrossDomainOutBox? outBox = null)
    : AsyncNotificationPublisher<UserLoginNotification>(publisher, outBox)
{
}