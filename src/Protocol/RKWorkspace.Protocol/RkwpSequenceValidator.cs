namespace RKWorkspace.Protocol;

public sealed class RkwpSequenceValidator : IRkwpSequenceValidator
{
    private readonly Dictionary<string, long> highestSequenceBySession = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> noncesBySession = new(StringComparer.Ordinal);
    private readonly IRkwpAuditSink? auditSink;

    public RkwpSequenceValidator(IRkwpAuditSink? auditSink = null)
    {
        this.auditSink = auditSink;
    }

    public void ValidateAndRecord(RkwpMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Nonce))
        {
            SecurityViolation(message, "NonceMissing", "Nonce is required.");
        }

        if (message.SequenceNumber <= 0)
        {
            SecurityViolation(message, "SequenceNumberInvalid", "SequenceNumber must be positive.");
        }

        if (highestSequenceBySession.TryGetValue(message.SessionId, out var highest) && message.SequenceNumber <= highest)
        {
            Replay(message, "Sequence number must monotonically increase within a session.");
        }

        if (!noncesBySession.TryGetValue(message.SessionId, out var nonces))
        {
            nonces = new HashSet<string>(StringComparer.Ordinal);
            noncesBySession[message.SessionId] = nonces;
        }

        if (!nonces.Add(message.Nonce))
        {
            Replay(message, "Nonce was already used within this session.");
        }

        highestSequenceBySession[message.SessionId] = message.SequenceNumber;
    }

    private void SecurityViolation(RkwpMessage message, string code, string details)
    {
        auditSink?.Write(RkwpAuditEvent.SecurityViolation(message.SessionId, message.LeaseId, code, details, message.Timestamp));
        throw new RkwpSecurityException($"{code}: {details}");
    }

    private void Replay(RkwpMessage message, string details)
    {
        auditSink?.Write(RkwpAuditEvent.ReplayDetected(message.SessionId, message.LeaseId, details, message.Timestamp));
        throw new RkwpSecurityException($"ReplayDetected: {details}");
    }
}
