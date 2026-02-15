using Avemepls.Auth.Abstractions.Models;
using Avemepls.ServiceBus.Common;
using Avemepls.ServiceBus.Events;

using MassTransit;

namespace Avemepls.Auth.Application.NotificationPublishers;

internal sealed class UserRegisteredViaPasswordNotificationPublisher(IPublishEndpoint publisher, CrossDomainOutBox? outBox = null)
    : AsyncNotificationPublisher<UserRegisteredViaPasswordNotification>(publisher, outBox)
{
}