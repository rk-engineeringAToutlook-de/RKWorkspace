namespace RKWorkspace.Protocol;

public sealed class InMemoryRkwpAuditSink : IRkwpAuditSink
{
    private readonly RkwpAuditTrail trail = new();

    public IReadOnlyList<RkwpAuditEvent> Events => trail.Events;

    public void Write(RkwpAuditEvent auditEvent)
    {
        trail.Add(auditEvent);
    }

    public bool Contains(RkwpAuditEventType eventType)
    {
        return trail.Contains(eventType);
    }
}
