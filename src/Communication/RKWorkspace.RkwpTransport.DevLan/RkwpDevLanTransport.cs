using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.DevLan;

public sealed class RkwpDevLanTransport : IRkwpTransport
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;
    private int _activeConnections;
    private int _messagesSent;
    private int _messagesReceived;
    private int _errors;
    private string _lastError = string.Empty;
    private TransportState _state = TransportState.Created;

    public RkwpDevLanTransport(RkwpDevLanOptions? options = null)
    {
        Options = options ?? new RkwpDevLanOptions();
    }

    public RkwpDevLanOptions Options { get; }

    public RkwpTransportMode Mode => RkwpTransportMode.LocalNetworkDev;

    public TransportState State => _state;

    public IRkwpTransportServer CreateServer(TransportEndpoint endpoint)
    {
        return new RkwpDevLanTransportServer(
            endpoint,
            Options,
            SetState,
            RecordConnection,
            RecordDisconnect,
            RecordReceived,
            RecordSent,
            RecordError);
    }

    public IRkwpTransportClient CreateClient(TransportEndpoint endpoint)
    {
        return new RkwpDevLanTransportClient(
            endpoint,
            Options,
            SetState,
            RecordConnection,
            RecordDisconnect,
            RecordReceived,
            RecordSent,
            RecordError);
    }

    public RkwpTransportDiagnostics GetDiagnostics()
    {
        return new RkwpTransportDiagnostics(
            Mode,
            State,
            _activeConnections,
            _messagesSent,
            _messagesReceived,
            _errors,
            _startedAt,
            _lastError);
    }

    private void SetState(TransportState state)
    {
        _state = state;
    }

    private void RecordConnection()
    {
        Interlocked.Increment(ref _activeConnections);
    }

    private void RecordDisconnect()
    {
        if (_activeConnections > 0)
        {
            Interlocked.Decrement(ref _activeConnections);
        }
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
