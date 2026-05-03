using MassTransit;

using Microsoft.Extensions.Logging;

namespace Avemepls.ServiceBus.Observers;

/// <summary>
/// Observer для логирования всех MassTransit консьюмеров
/// </summary>
public class ConsumerLoggingObserver(ILogger<ConsumerLoggingObserver> logger) : IConsumeObserver
{
    public Task PreConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        logger.LogTrace("Started processing {MessageType} [MessageId: {MessageId}]",
            typeof(T).Name,
            context.MessageId);

        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        var elapsedTime = context.ReceiveContext.ElapsedTime;
        var queueName = context.ReceiveContext.InputAddress?.AbsoluteUri ?? "unknown";

        logger.LogInformation(
            "Successfully processed {MessageType} in {ElapsedMs}ms [MessageId: {MessageId}, Queue: {Queue}]",
            typeof(T).Name,
            elapsedTime.TotalMilliseconds,
            context.MessageId,
            queueName);

        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        where T : class
    {
        var queueName = context.ReceiveContext.InputAddress?.AbsoluteUri ?? "unknown";

        logger.LogError(exception,
            "Failed to process {MessageType} [MessageId: {MessageId}, Queue: {Queue}]",
            typeof(T).Name,
            context.MessageId,
            queueName);

        return Task.CompletedTask;
    }
}