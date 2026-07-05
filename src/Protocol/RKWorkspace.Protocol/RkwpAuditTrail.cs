namespace RKWorkspace.Protocol;

public sealed class RkwpAuditTrail
{
    private readonly List<RkwpAuditEvent> events = new();

    public IReadOnlyList<RkwpAuditEvent> Events => events;

    public void Add(RkwpAuditEvent auditEvent)
    {
        events.Add(auditEvent);
    }

    public bool Contains(RkwpAuditEventType eventType)
    {
        return events.Any(auditEvent => auditEvent.EventType == eventType);
    }
}
