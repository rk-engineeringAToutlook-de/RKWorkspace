namespace RKWorkspace.Transport;

public interface ITransportClient
{
    TransportEndpoint Endpoint { get; }

    TransportState State { get; }

    Task<TransportResult> ConnectAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    Task<TransportResult> DisconnectAsync(CancellationToken cancellationToken = default);

    Task<TransportResult> SendAsync(
        TransportMessage message,
        CancellationToken cancellationToken = default);

    Task<TransportResult> ReceiveAsync(CancellationToken cancellationToken = default);

    Task<TransportResult> RequestAsync(
        TransportMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
