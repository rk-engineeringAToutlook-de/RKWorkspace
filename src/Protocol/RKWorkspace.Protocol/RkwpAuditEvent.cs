namespace RKWorkspace.Protocol;

public sealed record RkwpAuditEvent(
    string EventId,
    RkwpAuditEventType EventType,
    string SessionId,
    string? LeaseId,
    string? ThingId,
    string Message,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string> Metadata)
{
    public static RkwpAuditEvent Create(
        RkwpAuditEventType eventType,
        string sessionId,
        string? leaseId,
        string? thingId,
        string message,
        DateTimeOffset timestamp,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        return new RkwpAuditEvent(
            $"audit-{Guid.NewGuid():N}",
            eventType,
            sessionId,
            leaseId,
            thingId,
            message,
            timestamp,
            metadata ?? new Dictionary<string, string>());
    }

    public static RkwpAuditEvent SecurityViolation(string sessionId, string? leaseId, string code, string message, DateTimeOffset timestamp)
    {
        return Create(
            RkwpAuditEventType.SecurityViolation,
            sessionId,
            leaseId,
            null,
            message,
            timestamp,
            new Dictionary<string, string> { ["code"] = code });
    }

    public static RkwpAuditEvent ReplayDetected(string sessionId, string? leaseId, string message, DateTimeOffset timestamp)
    {
        return Create(RkwpAuditEventType.ReplayDetected, sessionId, leaseId, null, message, timestamp);
    }
}
