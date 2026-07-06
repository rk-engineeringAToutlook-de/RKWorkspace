using System.Net;
using System.Net.Sockets;
using System.Text;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.DevLan;

public sealed class RkwpDevLanTransportServer : IRkwpTransportServer
{
    private readonly RkwpDevLanOptions _options;
    private readonly Action<TransportState> _setState;
    private readonly Action _recordConnection;
    private readonly Action _recordDisconnect;
    private readonly Action _recordReceived;
    private readonly Action _recordSent;
    private readonly Action<string> _recordError;
    private TcpListener? _listener;
    private TcpClient? _currentClient;
    private NetworkStream? _currentStream;

    public RkwpDevLanTransportServer(
        TransportEndpoint endpoint,
        RkwpDevLanOptions options,
        Action<TransportState> setState,
        Action recordConnection,
        Action recordDisconnect,
        Action recordReceived,
        Action recordSent,
        Action<string> recordError)
    {
        Endpoint = endpoint;
        _options = options;
        _setState = setState;
        _recordConnection = recordConnection;
        _recordDisconnect = recordDisconnect;
        _recordReceived = recordReceived;
        _recordSent = recordSent;
        _recordError = recordError;
    }

    public TransportEndpoint Endpoint { get; }

    public TransportState State { get; private set; } = TransportState.Created;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var (host, port) = RkwpDevLanEndpoint.Parse(Endpoint);
        var address = IPAddress.Parse(host);
        State = TransportState.Starting;
        _setState(State);
        _listener = new TcpListener(address, port);
        _listener.Start();
        State = TransportState.Running;
        _setState(State);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        State = TransportState.Stopping;
        _setState(State);
        _currentStream?.Dispose();
        _currentClient?.Dispose();
        _listener?.Stop();
        State = TransportState.Stopped;
        _setState(State);
        return Task.CompletedTask;
    }

    public async Task<RkwpTransportMessage> WaitForMessageAsync(CancellationToken cancellationToken = default)
    {
        if (_listener is null)
        {
            throw new RkwpTransportException("DevLan server is not started.");
        }

        try
        {
            _currentClient = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            _recordConnection();
            State = TransportState.Connected;
            _setState(State);
            _currentStream = _currentClient.GetStream();
            using var reader = new StreamReader(_currentStream, Encoding.UTF8, leaveOpen: true);
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new RkwpTransportException("DevLan request was empty.");
            }

            var request = RkwpDevLanJson.Deserialize(line);
            _recordReceived();
            return request;
        }
        catch (Exception ex) when (ex is OperationCanceledException or SocketException or IOException or RkwpDevLanTransportException)
        {
            State = TransportState.Failed;
            _setState(State);
            _recordError(ex.Message);
            throw new RkwpTransportException(ex.Message, ex);
        }
    }

    public async Task SendResponseAsync(
        RkwpTransportMessage response,
        CancellationToken cancellationToken = default)
    {
        if (_currentClient is null || _currentStream is null)
        {
            throw new RkwpTransportException("DevLan server has no active client.");
        }

        try
        {
            await using var writer = new StreamWriter(_currentStream, new UTF8Encoding(false), leaveOpen: true)
            {
                AutoFlush = true
            };
            await writer.WriteLineAsync(RkwpDevLanJson.Serialize(response).AsMemory(), cancellationToken).ConfigureAwait(false);
            _recordSent();
        }
        finally
        {
            _currentStream.Dispose();
            _currentClient.Dispose();
            _currentStream = null;
            _currentClient = null;
            _recordDisconnect();
            State = TransportState.Running;
            _setState(State);
        }
    }
}
