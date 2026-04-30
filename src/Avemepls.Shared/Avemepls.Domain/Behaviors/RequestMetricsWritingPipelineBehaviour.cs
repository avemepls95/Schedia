using System.Diagnostics;
using System.Diagnostics.Metrics;

using MediatR;

namespace Avemepls.Domain.Behaviors;

public class RequestMetricsWritingPipelineBehaviour<TRequest, TResponse>(
    IMeterFactory meterFactory) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static Histogram<double>? _duration;
    private static Counter<long>? _count;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        EnsureMetersCreated(meterFactory);

        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            RecordMetrics(requestName, stopwatch.Elapsed.TotalSeconds, "ok");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordMetrics(requestName, stopwatch.Elapsed.TotalSeconds, "error", ex.GetType().Name);

            throw;
        }
    }

    private static void EnsureMetersCreated(IMeterFactory factory)
    {
        if (_duration is not null)
        {
            return;
        }

        var meter = factory.Create("Avemepls.Domain");

        _duration = meter.CreateHistogram<double>(
            "mediatr.request.duration",
            unit: "s",
            description: "Duration of MediatR request processing");

        _count = meter.CreateCounter<long>(
            "mediatr.request.count",
            description: "Number of MediatR requests processed");
    }

    private static void RecordMetrics(string requestName, double durationSeconds, string status, string? exceptionType = null)
    {
        var tags = new TagList
        {
            { "mediatr.request_name", requestName },
            { "mediatr.status", status },
        };

        if (exceptionType is not null)
        {
            tags.Add("mediatr.exception_type", exceptionType);
        }

        _duration!.Record(durationSeconds, tags);
        _count!.Add(1, tags);
    }
}