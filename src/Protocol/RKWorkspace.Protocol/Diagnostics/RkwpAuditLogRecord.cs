using System.Text.Json.Serialization;

namespace RKWorkspace.Protocol.Diagnostics;

public sealed record RkwpAuditLogRecord(
    string EventId,
    DateTimeOffset Timestamp,
    RkwpAuditEventType EventType,
    string SessionId,
    string? LeaseId,
    string? FrameSessionId,
    string? ThingId,
    string SourceAblageId,
    string TargetAblageId,
    RkwpAuditSeverity Severity,
    string Message,
    IReadOnlyDictionary<string, string> Metadata)
{
    [JsonIgnore]
    public bool IsPolicyDenied => EventType == RkwpAuditEventType.PolicyDenied;

    [JsonIgnore]
    public bool IsSecurityViolation => EventType is RkwpAuditEventType.SecurityViolation or RkwpAuditEventType.ReplayDetected;

    public static RkwpAuditLogRecord Create(
        RkwpAuditEventType eventType,
        string sessionId,
        string sourceAblageId,
        string targetAblageId,
        string message,
        DateTimeOffset timestamp,
        string? leaseId = null,
        string? frameSessionId = null,
        string? thingId = null,
        RkwpAuditSeverity severity = RkwpAuditSeverity.Info,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        return new RkwpAuditLogRecord(
            $"audit-{Guid.NewGuid():N}",
            timestamp,
            eventType,
            sessionId,
            leaseId,
            frameSessionId,
            thingId,
            sourceAblageId,
            targetAblageId,
            severity,
            message,
            metadata ?? new Dictionary<string, string>());
    }
}
