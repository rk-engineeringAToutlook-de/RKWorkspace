namespace RKWorkspace.Transport.Rkwp;

public interface IRkwpTransportServer
{
    TransportEndpoint Endpoint { get; }

    TransportState State { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task<RkwpTransportMessage> WaitForMessageAsync(CancellationToken cancellationToken = default);

    Task SendResponseAsync(
        RkwpTransportMessage response,
        CancellationToken cancellationToken = default);
}
