using System.Text.Json;

using Avemepls.Auditor.Core.Models;
using Avemepls.Auditor.DataAccess.Models;

using Mapster;

namespace Avemepls.Auditor.Builders;

public class AuditEventBuilder(AuditEvent? eventParameters = null)
{
    public const string PropertyChangedEventType = "PropertyChanged";

    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    protected AuditEvent EventParameters { get; } = eventParameters ?? new AuditEvent();

    static AuditEventBuilder()
    {
        TypeAdapterConfig<AuditEvent, AuditEvent>.NewConfig()
            .Ignore(r => r.IpAddress!)
            .AfterMapping((src, dest) => dest.IpAddress = src.IpAddress)
            .ShallowCopyForSameType(false); // for clone in build
    }

    public AuditEventBuilder WithSource(string source)
    {
        EventParameters.Source = source;

        return this;
    }

    internal AuditEventBuilder WithEntity<TEntity>()
    {
        EventParameters.EntityType = typeof(TEntity).Name;

        return this;
    }

    internal AuditEventBuilder WithEntity<TEntity, TId>(TId? id)
    {
        EventParameters.EntityType = typeof(TEntity).Name;

        if (id is not null)
        {
            EventParameters.EntityId = Convert.ToString(id);
        }

        return this;
    }

    /// <summary>
    /// Specifies event severity. See <see cref="AuditEventSeverity"/> for details
    /// </summary>
    public AuditEventBuilder WithSeverity(AuditEventSeverity severity)
    {
        EventParameters.Severity = severity;

        return this;
    }

    /// <summary>
    /// Specifies inner or child (more detailed) entity this event is related to
    /// </summary>
    /// <param name="id">Id of entity</param>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TId">Type of id</typeparam>
    public AuditEventBuilder ForInnerEntity<TEntity, TId>(TId id)
    {
        EventParameters.InnerEntityType = typeof(TEntity).Name;
        EventParameters.InnerEntityId = Convert.ToString(id);

        return this;
    }

    /// <summary>
    /// Set type of event
    /// </summary>
    internal AuditEventBuilder EventType(string eventType)
    {
        EventParameters.EventType = eventType;

        return this;
    }

    /// <summary>
    /// Set message for audit event
    /// </summary>
    public AuditEventBuilder Message(string message)
    {
        EventParameters.Message = message;

        return this;
    }

    /// <summary>
    /// Set payload for audit event (json, html, source message etc)
    /// </summary>
    public AuditEventBuilder Payload(string payload)
    {
        EventParameters.Payload = payload;

        return this;
    }

    /// <summary>
    /// Set object as audit event json payload
    /// </summary>
    public AuditEventBuilder AddJsonPayload(object payload)
    {
        EventParameters.Payload = JsonSerializer.Serialize(payload, _serializerOptions);

        return this;
    }

    /// <summary>
    /// Add audit event if property value changed. If no value change occured, event wouldn't be added
    /// </summary>
    /// <param name="propertyName">Name of property</param>
    /// <param name="oldValue">Old property value</param>
    /// <param name="newValue">New property value</param>
    public AuditEventBuilder PropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        PropertyChanged(propertyName, new PropertyValue(oldValue), new PropertyValue(newValue));

        return this;
    }

    /// <summary>
    /// Add audit event if property value changed. If no value change occured, event wouldn't be added
    /// </summary>
    /// <param name="propertyName">Name of property</param>
    /// <param name="oldValue">Old property value including accessor to human-readable value</param>
    /// <param name="newValue">New property value including accessor to human-readable value</param>
    public virtual AuditEventBuilder PropertyChanged(
        string propertyName,
        PropertyValue oldValue,
        PropertyValue newValue)
    {
        if (Equals(oldValue, newValue))
            return this;

        EventParameters.EventType = string.IsNullOrEmpty(EventParameters.EventType)
            ? PropertyChangedEventType
            : EventParameters.EventType;

        EventParameters.Parameter = propertyName;
        EventParameters.OldValue = oldValue.Value;
        EventParameters.NewValue = newValue.Value;
        EventParameters.OldValueReadable = oldValue.ReadableValue;
        EventParameters.NewValueReadable = newValue.ReadableValue;

        return this;
    }

    /// <summary>
    /// Property value descriptor. Includes both "raw" value and human-readable
    /// </summary>
    public record PropertyValue
    {
        /// <summary>
        /// Value of property
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// Human-readable value of property
        /// </summary>
        public string? ReadableValue => ReadableValueAccessor?.Invoke() ?? Value;

        private Func<string>? ReadableValueAccessor { get; }

        public PropertyValue(object? value, Func<string>? readableValueAccessor = null)
        {
            Value = value is null
                ? null
                : Convert.ToString(value);

            ReadableValueAccessor = readableValueAccessor;
        }
    }

    /// <summary>
    /// Makes a clone for event parameters
    /// </summary>
    internal AuditEvent Build()
    {
        return EventParameters.Adapt<AuditEvent>();
    }
}