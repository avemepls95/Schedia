using System.Collections.Immutable;

namespace Avemepls.Core.Models;

public static class ScopeContext
{
    private static readonly AsyncLocal<ImmutableStack<ScopeData>> ScopesDatas = new();

    private static string NamesSeparator => ":";

    public static string? Name
    {
        get => ScopesDatas.Value == null || ScopesDatas.Value.IsEmpty
            ? null
            : string.Join(NamesSeparator, ScopesDatas.Value!.Reverse()
                              .Where(d => d.Name is not null)
                              .Select(d => d.Name));
    }

    public static Dictionary<string, object> Data
    {
        get => ScopesDatas.Value == null || ScopesDatas.Value.IsEmpty
            ? new Dictionary<string, object>()
            : ScopesDatas.Value!.Reverse().SelectMany(d => d.Data).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IDisposable Enter(string name, Dictionary<string, object>? payload = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument cannot be null or whitespace.", nameof(name));
        }

        if (name.Contains(NamesSeparator))
        {
            throw new ArgumentException($"Argument cannot contain symbol '{NamesSeparator}'", nameof(name));
        }

        ScopesDatas.Value ??= ImmutableStack.Create<ScopeData>();

        ValidatePayload(payload);

        ScopesDatas.Value = ScopesDatas.Value.Push(new ScopeData
        {
            Name = name,
            Data = payload ?? new Dictionary<string, object>()
        });

        return new DisposableScope(() =>
        {
            if (!ScopesDatas.Value.IsEmpty)
            {
                ScopesDatas.Value = ScopesDatas.Value.Pop();
            }
        });
    }

    public static IDisposable Enter(Dictionary<string, object> payload)
    {
        if (payload == null)
        {
            throw new ArgumentException("Argument is null", nameof(payload));
        }

        ScopesDatas.Value ??= ImmutableStack.Create<ScopeData>();

        ValidatePayload(payload);

        ScopesDatas.Value = ScopesDatas.Value.Push(new ScopeData
        {
            Data = payload
        });

        return new DisposableScope(() =>
        {
            if (!ScopesDatas.Value.IsEmpty)
            {
                ScopesDatas.Value = ScopesDatas.Value.Pop();
            }
        });
    }

    private static void ValidatePayload(Dictionary<string, object>? parameters)
    {
        var existKeys = parameters?
            .Where(p => Data.ContainsKey(p.Key))
            .Select(p => p.Key)
            .ToArray();

        if (existKeys?.Any() == true)
        {
            throw new ArgumentException($"Keys {string.Join(", ", existKeys)} already added to scope");
        }
    }

#pragma warning disable S3881
    private sealed class DisposableScope(Action disposeAction) : IDisposable
#pragma warning restore S3881
    {
        public void Dispose() => disposeAction?.Invoke();
    }

    private sealed class ScopeData
    {
        public string? Name { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}