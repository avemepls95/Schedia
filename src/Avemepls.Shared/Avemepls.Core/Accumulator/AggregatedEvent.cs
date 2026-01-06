namespace Avemepls.Core.Accumulator;

/// <summary>
/// Accumulated information from several events in queue regarding accumulation policy
/// </summary>
public class AggregatedEvent
{
    /// <summary>
    /// Name of queue where events are coming to
    /// </summary>
    public string QueueName
    {
        get;
    }

    /// <summary>
    /// Subscriber id
    /// </summary>
    public string Subscriber
    {
        get;
    }

    public object[] Payloads
    {
        get;
    }

    public virtual object[] GetPayloads() => Payloads;

    public AggregatedEvent(string queueName, string subscriber, object[] payloads)
    {
        QueueName = queueName;
        Subscriber = subscriber;
        Payloads = payloads;
    }

    public AggregatedEvent(string queueName, string subscriber, IEnumerable<object> payloads)
    {
        QueueName = queueName;
        Subscriber = subscriber;
        Payloads = payloads.ToArray();
    }
}