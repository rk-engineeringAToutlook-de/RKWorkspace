namespace RKWorkspace.Transport;

public interface ITransportServer
{
    TransportEndpoint Endpoint { get; }

    TransportState State { get; }

    Task<TransportResult> StartAsync(CancellationToken cancellationToken = default);

    Task<TransportResult> StopAsync(CancellationToken cancellationToken = default);

    Task<TransportResult> WaitForMessageAsync(CancellationToken cancellationToken = default);

    Task<TransportResult> SendResponseAsync(
        TransportMessage response,
        CancellationToken cancellationToken = default);
}
