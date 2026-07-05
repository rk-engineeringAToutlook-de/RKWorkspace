using RKWorkspace.Transport.NamedPipes;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.Transport.Dev;

public sealed class RkwpDevTransportClient : IRkwpTransportClient
{
    private readonly ITransportClient _inner;
    private readonly Action _recordReceived;
    private readonly Action _recordSent;
    private readonly Action<string> _recordError;

    public RkwpDevTransportClient(
        ITransportClient inner,
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

    public async Task<RkwpTransportMessage> RequestAsync(
        RkwpTransportMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _inner.RequestAsync(
            message.ToTransportMessage(),
            timeout,
            cancellationToken).ConfigureAwait(false);
        _recordSent();

        if (result.Message is not null)
        {
            _recordReceived();
            return RkwpTransportMessage.FromTransportMessage(result.Message);
        }

        var error = string.IsNullOrWhiteSpace(result.Error)
            ? "RKWP dev transport request failed."
            : result.Error;
        _recordError(error);
        throw new RkwpTransportException(error);
    }

    public async Task<TransportResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (_inner is not NamedPipeTransportClient rawClient)
        {
            return TransportResult.Failed("Raw RKWP dev transport is only available for NamedPipeDev.");
        }

        var result = await rawClient.SendRawAsync(rawMessage, timeout, cancellationToken).ConfigureAwait(false);
        if (result.Success || result.Message is not null)
        {
            _recordSent();
            if (result.Message is not null)
            {
                _recordReceived();
            }
        }
        else
        {
            _recordError(result.Error);
        }

        return result;
    }
}
