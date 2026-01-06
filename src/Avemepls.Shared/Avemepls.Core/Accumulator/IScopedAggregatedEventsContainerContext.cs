namespace Avemepls.Core.Accumulator;

public interface IScopedAggregatedEventsContainerContext
{
    private static readonly AsyncLocal<IScopedAggregatedEventsContainer> Instance = new();

    public static IScopedAggregatedEventsContainer? GetInstance() => Instance.Value;

    public static IScopedAggregatedEventsContainer? SetInstance(IScopedAggregatedEventsContainer? value)
        => Instance.Value = value!;
}