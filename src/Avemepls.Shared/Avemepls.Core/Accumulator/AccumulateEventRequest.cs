namespace Avemepls.Core.Accumulator;

#pragma warning disable SA1402

/// <summary>
/// Request to accumulate event
/// </summary>
public class AccumulateEventRequest<T> : AccumulateEventRequest
    where T : class
{
    /// <summary>
    /// Event payload
    /// </summary>
    public override T GetPayload() => (T)Payload;

    /// <summary>
    /// Create request to accumulate event
    /// </summary>
    /// <param name="queueName">
    /// Name of queue to separate events by areas/applications. For example if you need to use same event among subsystems.
    /// Typically subsystem name will be good enought to be specified as queue name. Do not forgot to check queue name in receiver.
    /// </param>
    /// <param name="receivers">List of receivers of event. Use short (less than 64 chars) identifiers to be able to understand recipient id</param>
    /// <param name="payload">Payload. Will be serialized and passed back in aggregated event</param>
    public AccumulateEventRequest(string queueName, string[] receivers, T payload) : base(
        queueName,
        receivers,
        payload!)
    {
    }

    /// <summary>
    /// Create request to accumulate event
    /// </summary>
    /// <param name="queueName">
    /// Name of queue to separate events by areas/applications. For example if you need to use same event among subsystems.
    /// Typically subsystem name will be good enought to be specified as queue name. Do not forgot to check queue name in receiver.
    /// </param>
    /// <param name="receiver">Receiver identifier. Use short (less than 64 chars) identifiers to be able to understand recipient id</param>
    /// <param name="payload">Payload. Will be serialized and passed back in aggregated event</param>
    public AccumulateEventRequest(string queueName, string receiver, T payload) : this(
        queueName,
        [receiver],
        payload)
    {
    }
}

/// <summary>
/// Request to accumulate event
/// </summary>
public class AccumulateEventRequest
{
    /// <summary>
    /// Name of queue
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Array of receivers this event needs to be delivered to
    /// </summary>
    public string[] Receivers { get; }

    /// <summary>
    /// Event payload
    /// </summary>
    public object Payload { get; }

    public virtual object GetPayload() => Payload;

    /// <summary>
    /// Create request to accumulate event
    /// </summary>
    /// <param name="queueName">
    /// Name of queue to separate events by areas/applications. For example if you need to use same event among subsystems.
    /// Typically subsystem name will be good enought to be specified as queue name. Do not forgot to check queue name in receiver.
    /// </param>
    /// <param name="receivers">List of receivers of event. Use short (less than 64 chars) identifiers to be able to understand recipient id</param>
    /// <param name="payload">Payload. Will be serialized and passed back in aggregated event</param>
    public AccumulateEventRequest(string queueName, string[] receivers, object payload)
    {
        QueueName = queueName;
        Receivers = receivers;
        Payload = payload;
    }

    /// <summary>
    /// Create request to accumulate event
    /// </summary>
    /// <param name="queueName">
    /// Name of queue to separate events by areas/applications. For example if you need to use same event among subsystems.
    /// Typically subsystem name will be good enought to be specified as queue name. Do not forgot to check queue name in receiver.
    /// </param>
    /// <param name="receiver">Receiver identifier. Use short (less than 64 chars) identifiers to be able to understand recipient id</param>
    /// <param name="payload">Payload. Will be serialized and passed back in aggregated event</param>
    public AccumulateEventRequest(string queueName, string receiver, object payload) : this(
        queueName,
        [receiver],
        payload)
    {
    }
}
#pragma warning restore SA1402