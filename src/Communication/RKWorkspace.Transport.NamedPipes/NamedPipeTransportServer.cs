using System.IO.Pipes;
using RKWorkspace.Transport;

namespace RKWorkspace.Transport.NamedPipes;

public sealed class NamedPipeTransportServer : ITransportServer
{
    private readonly NamedPipeTransportOptions _options;
    private NamedPipeServerStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private TransportState _state = TransportState.Created;

    public NamedPipeTransportServer(
        TransportEndpoint endpoint,
        NamedPipeTransportOptions? options = null)
    {
        Endpoint = endpoint.Validate();
        _options = (options ?? new NamedPipeTransportOptions()).Validate();
    }

    public TransportEndpoint Endpoint { get; }

    public TransportState State => _state;

    public Task<TransportResult> StartAsync(CancellationToken cancellationToken = default)
    {
        _state = TransportState.Running;
        return Task.FromResult(TransportResult.FromMessage(StatusMessage()));
    }

    public Task<TransportResult> StopAsync(CancellationToken cancellationToken = default)
    {
        DisposeConnection();
        _state = TransportState.Stopped;
        return Task.FromResult(TransportResult.FromMessage(StatusMessage()));
    }

    public async Task<TransportResult> WaitForMessageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_state == TransportState.Created)
            {
                await StartAsync(cancellationToken).ConfigureAwait(false);
            }

            DisposeConnection();
            _pipe = new NamedPipeServerStream(
                Endpoint.Address,
                PipeDirection.InOut,
                _options.MaxServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            await _pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            _reader = new StreamReader(_pipe, leaveOpen: true);
            _writer = new StreamWriter(_pipe, leaveOpen: true)
            {
                AutoFlush = true
            };

            var rawMessage = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return TransportResult.Failed("Transport message was empty.");
            }

            return TransportResult.FromMessage(TransportMessage.FromJson(rawMessage));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
    }

    public async Task<TransportResult> SendResponseAsync(
        TransportMessage response,
        CancellationToken cancellationToken = default)
    {
        if (_writer is null)
        {
            return TransportResult.Failed("Transport server has no active client connection.");
        }

        try
        {
            await _writer.WriteLineAsync(response.ToJson().AsMemory(), cancellationToken).ConfigureAwait(false);
            return TransportResult.FromResponse(response);
        }
        catch (Exception ex) when (ex is IOException or TransportException)
        {
            _state = TransportState.Failed;
            return TransportResult.Failed(ex.Message);
        }
        finally
        {
            DisposeConnection();
            if (_state != TransportState.Stopped)
            {
                _state = TransportState.Running;
            }
        }
    }

    private TransportMessage StatusMessage()
    {
        return TransportMessage.Create(
            TransportMessageType.AgentStatusResponse,
            "named-pipe-server",
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
