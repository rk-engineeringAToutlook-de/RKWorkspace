using System.Net.Sockets;
using System.Text;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.DevLan;

public sealed class RkwpDevLanTransportClient : IRkwpTransportClient
{
    private readonly RkwpDevLanOptions _options;
    private readonly Action<TransportState> _setState;
    private readonly Action _recordConnection;
    private readonly Action _recordDisconnect;
    private readonly Action _recordReceived;
    private readonly Action _recordSent;
    private readonly Action<string> _recordError;

    public RkwpDevLanTransportClient(
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

    public async Task<RkwpTransportMessage> RequestAsync(
        RkwpTransportMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var raw = RkwpDevLanJson.Serialize(message);
        var result = await SendRawAsync(raw, timeout, cancellationToken).ConfigureAwait(false);
        if (!result.Success || result.Message is null)
        {
            var error = string.IsNullOrWhiteSpace(result.Error)
                ? "DevLan request failed."
                : result.Error;
            throw new RkwpTransportException(error);
        }

        return RkwpTransportMessage.FromTransportMessage(result.Message);
    }

    public async Task<TransportResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveTimeout = timeout ?? _options.DefaultTimeout;
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(effectiveTimeout);

        try
        {
            var (host, port) = RkwpDevLanEndpoint.Parse(Endpoint);
            State = TransportState.Starting;
            _setState(State);

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeoutSource.Token).ConfigureAwait(false);
            _recordConnection();
            State = TransportState.Connected;
            _setState(State);

            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
            {
                AutoFlush = true
            };

            await writer.WriteLineAsync(rawMessage.AsMemory(), timeoutSource.Token).ConfigureAwait(false);
            _recordSent();

            var responseLine = await reader.ReadLineAsync(timeoutSource.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseLine))
            {
                throw new RkwpDevLanTransportException("DevLan response was empty.");
            }

            var response = RkwpDevLanJson.Deserialize(responseLine);
            _recordReceived();
            return TransportResult.FromMessage(response.ToTransportMessage());
        }
        catch (Exception ex) when (ex is OperationCanceledException or SocketException or IOException or RkwpDevLanTransportException)
        {
            State = TransportState.Failed;
            _setState(State);
            _recordError(ex.Message);
            return TransportResult.Failed(ex.Message);
        }
        finally
        {
            _recordDisconnect();
            if (State != TransportState.Failed)
            {
                State = TransportState.Stopped;
                _setState(State);
            }
        }
    }
}
