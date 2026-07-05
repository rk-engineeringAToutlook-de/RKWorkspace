namespace RKWorkspace.Protocol;

public sealed record RkwpEnvelope(
    RkwpMessage Message,
    bool IsDevelopmentMode = false)
{
    public string MessageId => Message.MessageId;

    public RkwpMessageType MessageType => Message.MessageType;

    public RkwpVersion ProtocolVersion => Message.ProtocolVersion;

    public string SessionId => Message.SessionId;

    public string? LeaseId => Message.LeaseId;

    public string SourceAblageId => Message.SourceAblageId;

    public string TargetAblageId => Message.TargetAblageId;

    public DateTimeOffset Timestamp => Message.Timestamp;

    public long SequenceNumber => Message.SequenceNumber;

    public string Nonce => Message.Nonce;

    public string? CorrelationId => Message.CorrelationId;

    public IReadOnlyDictionary<string, string> Payload => Message.Payload;

    public IReadOnlyDictionary<string, string> Headers => Message.Headers;

    public string? AuthTag => Message.AuthTag;

    public IReadOnlyList<RkwpError> Validate()
    {
        return RkwpMessageValidator.Validate(Message, IsDevelopmentMode);
    }
}
