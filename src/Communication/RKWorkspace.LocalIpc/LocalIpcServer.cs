using RKWorkspace.Transport;
using RKWorkspace.Transport.NamedPipes;

namespace RKWorkspace.LocalIpc;

public sealed class LocalIpcServer
{
    public LocalIpcServer(string pipeName)
    {
        PipeName = LocalIpcEndpoint.ValidatePipeName(pipeName);
    }

    public string PipeName { get; }

    public async Task RunAsync(
        Func<LocalIpcMessage, CancellationToken, Task<LocalIpcMessage>> handler,
        Func<LocalIpcMessage, bool>? stopAfterResponse = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var server = new NamedPipeTransportServer(TransportEndpoint.NamedPipe(PipeName));
        await server.StartAsync(cancellationToken).ConfigureAwait(false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            var response = await HandleMessageAsync(request, handler, cancellationToken).ConfigureAwait(false);
            await server.SendResponseAsync(
                LocalIpcTransportMapper.ToTransport(response),
                cancellationToken).ConfigureAwait(false);

            if (stopAfterResponse?.Invoke(response) == true)
            {
                return;
            }
        }
    }

    private static async Task<LocalIpcMessage> HandleMessageAsync(
        TransportResult request,
        Func<LocalIpcMessage, CancellationToken, Task<LocalIpcMessage>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!request.Success || request.Message is null)
            {
                return Error("local-ipc-server", "unknown", request.Error);
            }

            var message = LocalIpcTransportMapper.ToLocal(request.Message);
            return await handler(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is LocalIpcException or InvalidOperationException)
        {
            return Error("local-ipc-server", "unknown", ex.Message);
        }
    }

    public static LocalIpcMessage Error(string sourceAgentId, string targetAgentId, string message)
    {
        return LocalIpcMessage.Create(
            LocalIpcMessageType.ErrorResponse,
            sourceAgentId,
            targetAgentId,
            new Dictionary<string, string>
            {
                ["error"] = string.IsNullOrWhiteSpace(message) ? "IPC error." : message
            });
    }
}
