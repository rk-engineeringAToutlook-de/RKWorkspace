using System.Security.Cryptography;

namespace RKWorkspace.Protocol;

public sealed record RkwpMessage
{
    public required string MessageId { get; init; }

    public required RkwpMessageType MessageType { get; init; }

    public required RkwpVersion ProtocolVersion { get; init; }

    public required string SessionId { get; init; }

    public string? LeaseId { get; init; }

    public required string SourceAblageId { get; init; }

    public required string TargetAblageId { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required long SequenceNumber { get; init; }

    public required string Nonce { get; init; }

    public string? CorrelationId { get; init; }

    public IReadOnlyDictionary<string, string> Payload { get; init; } = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    public string? AuthTag { get; init; }

    public static RkwpMessage Create(
        RkwpMessageType messageType,
        RkwpSession session,
        long sequenceNumber,
        IReadOnlyDictionary<string, string>? payload = null,
        string? leaseId = null)
    {
        return new RkwpMessage
        {
            MessageId = $"rkwp-msg-{Guid.NewGuid():N}",
            MessageType = messageType,
            ProtocolVersion = session.ProtocolVersion,
            SessionId = session.SessionId,
            LeaseId = leaseId,
            SourceAblageId = session.OwnerAblageId,
            TargetAblageId = session.GuestAblageId,
            Timestamp = DateTimeOffset.UtcNow,
            SequenceNumber = sequenceNumber,
            Nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
            CorrelationId = null,
            Payload = payload ?? new Dictionary<string, string>(),
            Headers = new Dictionary<string, string>
            {
                ["security"] = session.SecureSessionRequired ? "required" : "development"
            }
        };
    }
}
