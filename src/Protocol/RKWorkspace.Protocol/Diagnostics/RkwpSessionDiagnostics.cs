namespace RKWorkspace.Protocol.Diagnostics;

public sealed record RkwpSessionDiagnostics(
    int ActiveSessions,
    int ActiveLeases,
    int ExpiredLeases,
    int RecoveredLeases,
    int FrameSessions,
    int Heartbeats,
    int PolicyDeniedEvents,
    int SecurityViolations,
    int OwnershipTransferRequests,
    int Revocations,
    bool NoFileIngressPassed)
{
    public bool IsHealthy => ActiveSessions > 0 && ActiveLeases >= 0 && FrameSessions > 0 && NoFileIngressPassed;

    public static RkwpSessionDiagnostics FromEvents(IEnumerable<RkwpAuditLogRecord> records)
    {
        var events = records.ToArray();
        var startedSessions = events
            .Where(auditEvent => auditEvent.EventType == RkwpAuditEventType.SessionStarted)
            .Select(auditEvent => auditEvent.SessionId)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var returnedLeases = events
            .Where(auditEvent => auditEvent.EventType == RkwpAuditEventType.FrameReturned)
            .Select(auditEvent => auditEvent.LeaseId)
            .Where(leaseId => !string.IsNullOrWhiteSpace(leaseId))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var grantedLeases = events
            .Where(auditEvent => auditEvent.EventType == RkwpAuditEventType.LeaseGranted)
            .Select(auditEvent => auditEvent.LeaseId)
            .Where(leaseId => !string.IsNullOrWhiteSpace(leaseId))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var recoveredLeases = events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.RecoveredByOwner);
        var noFileIngressPassed = events.Any(auditEvent =>
            auditEvent.EventType == RkwpAuditEventType.NoFileIngressChecked &&
            auditEvent.Metadata.TryGetValue("status", out var status) &&
            string.Equals(status, "success", StringComparison.OrdinalIgnoreCase));

        return new RkwpSessionDiagnostics(
            startedSessions,
            Math.Max(0, grantedLeases - returnedLeases - recoveredLeases),
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.LeaseExpired),
            recoveredLeases,
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.FrameOpened),
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.Heartbeat),
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.PolicyDenied),
            events.Count(auditEvent => auditEvent.IsSecurityViolation),
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.OwnershipTransferRequested),
            events.Count(auditEvent => auditEvent.EventType == RkwpAuditEventType.LeaseRevoked),
            noFileIngressPassed);
    }
}
