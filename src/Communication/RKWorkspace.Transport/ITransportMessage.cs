namespace RKWorkspace.Transport;

public interface ITransportMessage
{
    string MessageId { get; }

    TransportMessageType MessageType { get; }

    string SourceId { get; }

    string TargetId { get; }

    DateTimeOffset Timestamp { get; }

    IReadOnlyDictionary<string, string> Payload { get; }

    string CorrelationId { get; }

    IReadOnlyDictionary<string, string> Headers { get; }
}
