using System.IO.Pipes;
using RKWorkspace.Transport;

namespace RKWorkspace.Transport.NamedPipes;

public sealed class NamedPipeTransportClient : ITransportClient
{
    private readonly NamedPipeTransportOptions _options;
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private TransportState _state = TransportState.Created;

    public NamedPipeTransportClient(
        TransportEndpoint endpoint,
        NamedPipeTransportOptions? options = null)
    {
        Endpoint = endpoint.Validate();
        _options = (options ?? new NamedPipeTransportOptions()).Validate();
    }

    public TransportEndpoint Endpoint { get; }

    public TransportState State => _state;

    public async Task<TransportResult> ConnectAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (_state == TransportState.Connected)
        {
            return TransportResult.FromMessage(ConnectedMessage());
        }

        using var timeoutSource = new CancellationTokenSource(timeout ?? _options.DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutSource.Token,
            cancellationToken);

        try
        {
            _state = TransportState.Starting;
            _pipe = new NamedPipeClientStream(
                _options.ServerName,
                Endpoint.Address,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            await _pipe.ConnectAsync(linked.Token).ConfigureAwait(false);
            _reader = new StreamReader(_pipe, leaveOpen: true);
            _writer = new StreamWriter(_pipe, leaveOpen: true)
            {
                AutoFlush = true
            };
            _state = TransportState.Connected;
            return TransportResult.FromMessage(ConnectedMessage());
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed($"Transport request to '{Endpoint.Address}' timed out.", timedOut: true);
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
    }

    public Task<TransportResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        DisposeConnection();
        _state = TransportState.Stopped;
        return Task.FromResult(TransportResult.FromMessage(ConnectedMessage()));
    }

    public async Task<TransportResult> SendAsync(
        TransportMessage message,
        CancellationToken cancellationToken = default)
    {
        if (_writer is null)
        {
            return TransportResult.Failed("Transport client is not connected.");
        }

        try
        {
            await _writer.WriteLineAsync(message.ToJson().AsMemory(), cancellationToken).ConfigureAwait(false);
            return TransportResult.FromMessage(message);
        }
        catch (Exception ex) when (ex is IOException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
    }

    public async Task<TransportResult> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (_reader is null)
        {
            return TransportResult.Failed("Transport client is not connected.");
        }

        try
        {
            var responseJson = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return TransportResult.Failed("Transport server did not return a response.");
            }

            return TransportResult.FromResponse(TransportMessage.FromJson(responseJson));
        }
        catch (Exception ex) when (ex is IOException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
    }

    public async Task<TransportResult> RequestAsync(
        TransportMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var timeoutSource = new CancellationTokenSource(timeout ?? _options.DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutSource.Token,
            cancellationToken);

        try
        {
            var connect = await ConnectAsync(timeout, linked.Token).ConfigureAwait(false);
            if (!connect.Success)
            {
                return connect;
            }

            var send = await SendAsync(message, linked.Token).ConfigureAwait(false);
            if (!send.Success)
            {
                return send;
            }

            return await ReceiveAsync(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed($"Transport request to '{Endpoint.Address}' timed out.", timedOut: true);
        }
        finally
        {
            await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    public async Task<TransportResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var timeoutSource = new CancellationTokenSource(timeout ?? _options.DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutSource.Token,
            cancellationToken);

        try
        {
            var connect = await ConnectAsync(timeout, linked.Token).ConfigureAwait(false);
            if (!connect.Success)
            {
                return connect;
            }

            if (_writer is null || _reader is null)
            {
                return TransportResult.Failed("Transport client is not connected.");
            }

            await _writer.WriteLineAsync(rawMessage.AsMemory(), linked.Token).ConfigureAwait(false);
            var responseJson = await _reader.ReadLineAsync(linked.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return TransportResult.Failed("Transport server did not return a response.");
            }

            return TransportResult.FromResponse(TransportMessage.FromJson(responseJson));
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed($"Transport request to '{Endpoint.Address}' timed out.", timedOut: true);
        }
        catch (Exception ex) when (ex is IOException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
        finally
        {
            await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private TransportMessage ConnectedMessage()
    {
        return TransportMessage.Create(
            TransportMessageType.AgentStatusResponse,
            "named-pipe-client",
            Endpoint.EndpointId,
            new Dictionary<string, string>
            {
                ["state"] = _state.ToString()
            });
    }

    private void DisposeConnection()
    {
        TryDispose(_reader);
        TryDispose(_writer);
        TryDispose(_pipe);
        _reader = null;
        _writer = null;
        _pipe = null;
    }

    private static void TryDispose(IDisposable? disposable)
    {
        try
        {
            disposable?.Dispose();
        }
        catch (IOException)
        {
        }
    }
}
