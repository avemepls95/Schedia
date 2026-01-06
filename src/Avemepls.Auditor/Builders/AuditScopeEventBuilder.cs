using Avemepls.Auditor.DataAccess.Models;

namespace Avemepls.Auditor.Builders;

public class AuditScopeEventBuilder(AuditScope auditScope, AuditEvent eventParameters) : AuditEventBuilder(eventParameters)
{
    private bool _eventAdded;

    internal void Finish()
    {
        if (!_eventAdded)
            auditScope.AddEvent(Build());
    }

    public override AuditEventBuilder PropertyChanged(
        string propertyName,
        PropertyValue oldValue,
        PropertyValue newValue)
    {
        if (Equals(oldValue, newValue))
            return this;
        base.PropertyChanged(propertyName, oldValue, newValue);

        _eventAdded = true;
        auditScope.AddEvent(Build());

        EventParameters.EventType = string.Empty;
        EventParameters.Parameter = null;
        EventParameters.OldValue = EventParameters.NewValue = null;
        EventParameters.OldValueReadable = EventParameters.NewValueReadable = null;

        return this;
    }
}