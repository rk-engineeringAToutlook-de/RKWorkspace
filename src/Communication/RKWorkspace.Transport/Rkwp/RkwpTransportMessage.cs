namespace RKWorkspace.Transport.Rkwp;

public sealed record RkwpTransportMessage
{
    public required string MessageId { get; init; }

    public required TransportMessageType MessageType { get; init; }

    public required string SourceAblageId { get; init; }

    public required string TargetAblageId { get; init; }

    public string SessionId { get; init; } = string.Empty;

    public string CorrelationId { get; init; } = string.Empty;

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyDictionary<string, string> Payload { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static RkwpTransportMessage Create(
        TransportMessageType messageType,
        string sourceAblageId,
        string targetAblageId,
        string sessionId = "",
        IReadOnlyDictionary<string, string>? payload = null,
        string correlationId = "",
        IReadOnlyDictionary<string, string>? headers = null)
    {
        return new RkwpTransportMessage
        {
            MessageId = $"rkwp-transport-{Guid.NewGuid():N}",
            MessageType = messageType,
            SourceAblageId = sourceAblageId,
            TargetAblageId = targetAblageId,
            SessionId = sessionId,
            CorrelationId = correlationId,
            Payload = Copy(payload),
            Headers = Copy(headers)
        }.Validate();
    }

    public RkwpTransportMessage Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
        {
            throw new RkwpTransportException("RKWP transport message id is required.");
        }

        if (MessageType == TransportMessageType.Unknown)
        {
            throw new RkwpTransportException("RKWP transport message type must not be Unknown.");
        }

        if (string.IsNullOrWhiteSpace(SourceAblageId))
        {
            throw new RkwpTransportException("RKWP source ablage id is required.");
        }

        if (string.IsNullOrWhiteSpace(TargetAblageId))
        {
            throw new RkwpTransportException("RKWP target ablage id is required.");
        }

        return this with
        {
            Payload = Copy(Payload),
            Headers = Copy(Headers)
        };
    }

    public TransportMessage ToTransportMessage()
    {
        var payload = Copy(Payload);
        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            payload["sessionId"] = SessionId;
        }

        return new TransportMessage
        {
            MessageId = MessageId,
            MessageType = MessageType,
            SourceId = SourceAblageId,
            TargetId = TargetAblageId,
            Timestamp = Timestamp,
            Payload = payload,
            CorrelationId = CorrelationId,
            Headers = Copy(Headers)
        }.Validate();
    }

    public static RkwpTransportMessage FromTransportMessage(TransportMessage message)
    {
        var payload = Copy(message.Payload);
        payload.TryGetValue("sessionId", out var sessionId);
        payload.Remove("sessionId");

        return new RkwpTransportMessage
        {
            MessageId = message.MessageId,
            MessageType = message.MessageType,
            SourceAblageId = message.SourceId,
            TargetAblageId = message.TargetId,
            Timestamp = message.Timestamp,
            Payload = payload,
            SessionId = sessionId ?? string.Empty,
            CorrelationId = message.CorrelationId,
            Headers = Copy(message.Headers)
        }.Validate();
    }

    private static Dictionary<string, string> Copy(IReadOnlyDictionary<string, string>? values)
    {
        return values?.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
