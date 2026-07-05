namespace RKWorkspace.Transport.Rkwp;

public interface IRkwpTransport
{
    RkwpTransportMode Mode { get; }

    TransportState State { get; }

    IRkwpTransportServer CreateServer(TransportEndpoint endpoint);

    IRkwpTransportClient CreateClient(TransportEndpoint endpoint);

    RkwpTransportDiagnostics GetDiagnostics();
}
