using System.IO.Pipes;

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

        while (!cancellationToken.IsCancellationRequested)
        {
            await using var pipe = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

            using var reader = new StreamReader(pipe, leaveOpen: true);
            await using var writer = new StreamWriter(pipe, leaveOpen: true)
            {
                AutoFlush = true
            };

            var response = await HandleMessageAsync(reader, handler, cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync(response.ToJson().AsMemory(), cancellationToken).ConfigureAwait(false);

            if (stopAfterResponse?.Invoke(response) == true)
            {
                return;
            }
        }
    }

    private static async Task<LocalIpcMessage> HandleMessageAsync(
        TextReader reader,
        Func<LocalIpcMessage, CancellationToken, Task<LocalIpcMessage>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var rawMessage = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return Error("local-ipc-server", "unknown", "IPC message was empty.");
            }

            var message = LocalIpcMessage.FromJson(rawMessage);
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
