namespace RKWorkspace.Transport;

public interface ITransport
{
    string TransportId { get; }

    string DisplayName { get; }

    TransportState State { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    ITransportClient CreateClient(TransportEndpoint endpoint);

    ITransportServer CreateServer(TransportEndpoint endpoint);
}
