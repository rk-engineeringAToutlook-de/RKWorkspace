using RKWorkspace.Protocol;

namespace RKWorkspace.Protocol.Ownership;

public enum RkwpRevocationReason
{
    OwnerRequested,
    PolicyChanged,
    HeartbeatLost,
    SecurityViolation,
    Timeout,
    UserCancelled,
    GuestDisconnected,
    Unknown
}

public sealed record RkwpRevocationRequest(
    string RequestId,
    string SessionId,
    string LeaseId,
    RkwpRevocationReason Reason,
    DateTimeOffset RequestedAt);

public sealed record RkwpRevocationResult(
    string RequestId,
    CarryLease Lease,
    FrameSession FrameSession,
    bool GuestFrameInvalid,
    bool OwnerUnlockedThing,
    RkwpRevocationReason Reason);

public static class RkwpRevocationService
{
    public static RkwpRevocationResult Revoke(
        RkwpRevocationRequest request,
        CarryLease lease,
        FrameSession frameSession,
        IRkwpAuditSink? auditSink = null)
    {
        if (!string.Equals(request.SessionId, lease.SessionId, StringComparison.Ordinal) ||
            !string.Equals(request.LeaseId, lease.LeaseId, StringComparison.Ordinal))
        {
            auditSink?.Write(RkwpAuditEvent.Create(
                RkwpAuditEventType.SecurityViolation,
                request.SessionId,
                request.LeaseId,
                lease.ThingId,
                "Revocation request is not bound to the active lease.",
                request.RequestedAt));

            throw new RkwpSecurityException("Revocation request is not bound to the active lease.");
        }

        var revokedLease = lease.Revoke(request.RequestedAt);
        var revokedFrame = frameSession.Revoke(request.RequestedAt);

        auditSink?.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.LeaseRevoked,
            lease.SessionId,
            lease.LeaseId,
            lease.ThingId,
            $"Lease revoked: {request.Reason}.",
            request.RequestedAt,
            new Dictionary<string, string> { ["reason"] = request.Reason.ToString() }));

        return new RkwpRevocationResult(
            request.RequestId,
            revokedLease,
            revokedFrame,
            GuestFrameInvalid: true,
            OwnerUnlockedThing: true,
            request.Reason);
    }
}
