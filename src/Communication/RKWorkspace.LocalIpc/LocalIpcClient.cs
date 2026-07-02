using RKWorkspace.Transport;
using RKWorkspace.Transport.NamedPipes;

namespace RKWorkspace.LocalIpc;

public sealed class LocalIpcClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    public LocalIpcClient(string pipeName)
    {
        PipeName = LocalIpcEndpoint.ValidatePipeName(pipeName);
    }

    public string PipeName { get; }

    public Task<LocalIpcResult> SendAsync(
        LocalIpcMessage message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        return SendTransportAsync(
            client,
            LocalIpcTransportMapper.ToTransport(message),
            timeout,
            cancellationToken);
    }

    public async Task<LocalIpcResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        return LocalIpcResult.FromTransport(await client
            .SendRawAsync(rawMessage, timeout ?? DefaultTimeout, cancellationToken)
            .ConfigureAwait(false));
    }

    private NamedPipeTransportClient CreateClient()
    {
        return new NamedPipeTransportClient(
            TransportEndpoint.NamedPipe(PipeName),
            new NamedPipeTransportOptions
            {
                DefaultTimeout = DefaultTimeout
            });
    }

    private static async Task<LocalIpcResult> SendTransportAsync(
        ITransportClient client,
        TransportMessage message,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        return LocalIpcResult.FromTransport(await client
            .RequestAsync(message, timeout ?? DefaultTimeout, cancellationToken)
            .ConfigureAwait(false));
    }
}
