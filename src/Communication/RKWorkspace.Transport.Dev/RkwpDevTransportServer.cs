using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.Transport.Dev;

public sealed class RkwpDevTransportServer : IRkwpTransportServer
{
    private readonly ITransportServer _inner;
    private readonly Action _recordReceived;
    private readonly Action _recordSent;
    private readonly Action<string> _recordError;

    public RkwpDevTransportServer(
        ITransportServer inner,
        Action recordReceived,
        Action recordSent,
        Action<string> recordError)
    {
        _inner = inner;
        _recordReceived = recordReceived;
        _recordSent = recordSent;
        _recordError = recordError;
    }

    public TransportEndpoint Endpoint => _inner.Endpoint;

    public TransportState State => _inner.State;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var result = await _inner.StartAsync(cancellationToken).ConfigureAwait(false);
        EnsureSuccess(result, "start server");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var result = await _inner.StopAsync(cancellationToken).ConfigureAwait(false);
        EnsureSuccess(result, "stop server");
    }

    public async Task<RkwpTransportMessage> WaitForMessageAsync(CancellationToken cancellationToken = default)
    {
        var result = await _inner.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
        if (result.Message is null)
        {
            var error = string.IsNullOrWhiteSpace(result.Error)
                ? "RKWP dev transport server did not receive a message."
                : result.Error;
            _recordError(error);
            throw new RkwpTransportException(error);
        }

        _recordReceived();
        return RkwpTransportMessage.FromTransportMessage(result.Message);
    }

    public async Task SendResponseAsync(
        RkwpTransportMessage response,
        CancellationToken cancellationToken = default)
    {
        var result = await _inner.SendResponseAsync(
            response.ToTransportMessage(),
            cancellationToken).ConfigureAwait(false);
        if (result.Message is null)
        {
            EnsureSuccess(result, "send response");
        }

        _recordSent();
    }

    private void EnsureSuccess(TransportResult result, string operation)
    {
        if (result.Success || result.Message is not null)
        {
            return;
        }

        var error = string.IsNullOrWhiteSpace(result.Error)
            ? $"RKWP dev transport failed to {operation}."
            : result.Error;
        _recordError(error);
        throw new RkwpTransportException(error);
    }
}
