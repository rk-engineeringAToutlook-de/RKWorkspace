using RKWorkspace.Transport;

namespace RKWorkspace.Transport.NamedPipes;

public sealed class NamedPipeTransport : ITransport
{
    private readonly NamedPipeTransportOptions _options;
    private TransportState _state = TransportState.Created;

    public NamedPipeTransport(NamedPipeTransportOptions? options = null)
    {
        _options = (options ?? new NamedPipeTransportOptions()).Validate();
    }

    public string TransportId => "named-pipe";

    public string DisplayName => "Named Pipe Transport";

    public TransportState State => _state;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _state = TransportState.Running;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _state = TransportState.Stopped;
        return Task.CompletedTask;
    }

    public ITransportClient CreateClient(TransportEndpoint endpoint)
    {
        return new NamedPipeTransportClient(endpoint, _options);
    }

    public ITransportServer CreateServer(TransportEndpoint endpoint)
    {
        return new NamedPipeTransportServer(endpoint, _options);
    }
}
