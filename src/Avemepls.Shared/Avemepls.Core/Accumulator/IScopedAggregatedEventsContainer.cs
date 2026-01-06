namespace Avemepls.Core.Accumulator;

public interface IScopedAggregatedEventsContainer
{
    Task Accumulate(AccumulateEventRequest request, CancellationToken cancellationToken);

    Task DeliverEvents(
        Func<AggregatedEvent, Task> eventDeliveryCallback,
        CancellationToken cancellationToken);
}