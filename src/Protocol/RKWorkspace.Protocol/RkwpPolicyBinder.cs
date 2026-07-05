using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Protocol;

public sealed class RkwpPolicyBinder : IRkwpPolicyBinder
{
    private readonly IRkwpAuditSink? auditSink;

    public RkwpPolicyBinder(IRkwpAuditSink? auditSink = null)
    {
        this.auditSink = auditSink;
    }

    public PolicyBindingValidation Validate(CarryLease lease, FrameSession frameSession)
    {
        if (!string.Equals(lease.PolicyId, frameSession.PolicyId, StringComparison.Ordinal) ||
            lease.PolicyVersion != frameSession.PolicyVersion ||
            !string.Equals(lease.PolicyHash, frameSession.PolicyHash, StringComparison.Ordinal))
        {
            auditSink?.Write(RkwpAuditEvent.Create(
                RkwpAuditEventType.PolicyDenied,
                lease.SessionId,
                lease.LeaseId,
                lease.ThingId,
                "Policy binding changed while frame session was active.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string>
                {
                    ["lease-policy"] = $"{lease.PolicyId}:{lease.PolicyVersion}",
                    ["frame-policy"] = $"{frameSession.PolicyId}:{frameSession.PolicyVersion}"
                }));

            return PolicyBindingValidation.Denied(lease.PolicyId, lease.PolicyVersion, "Policy binding mismatch.");
        }

        return PolicyBindingValidation.Valid(lease.PolicyId, lease.PolicyVersion);
    }
}
