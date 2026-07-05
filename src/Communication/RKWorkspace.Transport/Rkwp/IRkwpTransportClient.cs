namespace RKWorkspace.Transport.Rkwp;

public interface IRkwpTransportClient
{
    TransportEndpoint Endpoint { get; }

    TransportState State { get; }

    Task<RkwpTransportMessage> RequestAsync(
        RkwpTransportMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    Task<TransportResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
