using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.ServiceBus.Common;

using MassTransit;

using MediatR;

namespace Avemepls.ServiceBus.Events;

/// <summary>
/// Обработчик поставки события MediatR в шину
/// </summary>
public abstract class AsyncNotificationPublisher<TNotification>(
    IPublishEndpoint publisher,
    CrossDomainOutBox? outBox = null) : INotificationHandler<TNotification>
    where TNotification : IAsyncNotification, INotification
{
    public async Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (!CanPublish(notification))
        {
            return;
        }

        var mapped = await MapNotification(notification, cancellationToken);

        await publisher.Publish(mapped, cancellationToken);

        if (outBox?.DbContext is not null)
        {
            await outBox.DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    protected virtual ValueTask<object> MapNotification(TNotification notification, CancellationToken cancellationToken)
        => ValueTask.FromResult<object>(notification);

    protected virtual bool CanPublish(TNotification notification) => true;
}

/// <summary>
/// Обработчик поставки события MediatR в шину
/// </summary>
#pragma warning disable SA1402
public abstract class AsyncNotificationPublisher<TNotification, TBusNotification>(
    IMapper mapper,
    IPublishEndpoint publisher,
    CrossDomainOutBox? outBox = null)
    : INotificationHandler<TNotification>
#pragma warning restore SA1402
    where TNotification : INotification
    where TBusNotification : class, new()
{
    public async Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (!CanPublish(notification))
        {
            return;
        }

        var mapped = await MapNotification(notification, cancellationToken);

        await publisher.Publish(mapped, cancellationToken);

        if (outBox?.DbContext is not null)
        {
            await outBox.DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    protected virtual bool CanPublish(TNotification notification) => true;

    protected virtual ValueTask<TBusNotification> MapNotification(
        TNotification notification,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(mapper.Map<TBusNotification>(notification));
    }
}