using RKWorkspace.Transport;

namespace RKWorkspace.LocalIpc;

internal static class LocalIpcTransportMapper
{
    public static TransportMessage ToTransport(LocalIpcMessage message)
    {
        var messageType = Enum.Parse<TransportMessageType>(
            message.MessageType.ToString(),
            ignoreCase: true);

        return new TransportMessage
        {
            MessageId = message.MessageId,
            MessageType = messageType,
            SourceId = message.SourceAgentId,
            TargetId = message.TargetAgentId,
            Timestamp = message.Timestamp,
            Payload = message.Payload.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        }.Validate();
    }

    public static LocalIpcMessage ToLocal(TransportMessage message)
    {
        var messageType = Enum.TryParse<LocalIpcMessageType>(
            message.MessageType.ToString(),
            ignoreCase: true,
            out var parsed)
                ? parsed
                : LocalIpcMessageType.ErrorResponse;

        return new LocalIpcMessage
        {
            MessageId = message.MessageId,
            MessageType = messageType,
            SourceAgentId = message.SourceId,
            TargetAgentId = message.TargetId,
            Timestamp = message.Timestamp,
            Payload = message.Payload.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        }.Validate();
    }
}
