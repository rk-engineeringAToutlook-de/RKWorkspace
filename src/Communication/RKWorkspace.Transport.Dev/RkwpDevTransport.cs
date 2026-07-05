using RKWorkspace.Transport.NamedPipes;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.Transport.Dev;

public sealed class RkwpDevTransport : IRkwpTransport
{
    private readonly NamedPipeTransport _transport;
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;
    private int _messagesSent;
    private int _messagesReceived;
    private int _errors;
    private string _lastError = string.Empty;

    public RkwpDevTransport(NamedPipeTransportOptions? options = null)
    {
        _transport = new NamedPipeTransport(options);
    }

    public RkwpTransportMode Mode => RkwpTransportMode.NamedPipeDev;

    public TransportState State => _transport.State;

    public IRkwpTransportServer CreateServer(TransportEndpoint endpoint)
    {
        return new RkwpDevTransportServer(
            _transport.CreateServer(endpoint),
            RecordReceived,
            RecordSent,
            RecordError);
    }

    public IRkwpTransportClient CreateClient(TransportEndpoint endpoint)
    {
        return new RkwpDevTransportClient(
            _transport.CreateClient(endpoint),
            RecordReceived,
            RecordSent,
            RecordError);
    }

    public RkwpTransportDiagnostics GetDiagnostics()
    {
        return new RkwpTransportDiagnostics(
            Mode,
            State,
            ActiveConnections: 0,
            MessagesSent: _messagesSent,
            MessagesReceived: _messagesReceived,
            Errors: _errors,
            StartedAt: _startedAt,
            LastError: _lastError);
    }

    private void RecordSent()
    {
        Interlocked.Increment(ref _messagesSent);
    }

    private void RecordReceived()
    {
        Interlocked.Increment(ref _messagesReceived);
    }

    private void RecordError(string error)
    {
        Interlocked.Increment(ref _errors);
        _lastError = error;
    }
}
