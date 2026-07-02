using System.IO.Pipes;

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
        return SendRawAsync(message.ToJson(), timeout, cancellationToken);
    }

    public async Task<LocalIpcResult> SendRawAsync(
        string rawMessage,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var timeoutSource = new CancellationTokenSource(timeout ?? DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutSource.Token,
            cancellationToken);

        try
        {
            await using var pipe = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            await pipe.ConnectAsync(linked.Token).ConfigureAwait(false);

            using var reader = new StreamReader(pipe, leaveOpen: true);
            await using var writer = new StreamWriter(pipe, leaveOpen: true)
            {
                AutoFlush = true
            };

            await writer.WriteLineAsync(rawMessage.AsMemory(), linked.Token).ConfigureAwait(false);
            var responseJson = await reader.ReadLineAsync(linked.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return LocalIpcResult.Failed("IPC server did not return a response.");
            }

            return LocalIpcResult.FromResponse(LocalIpcMessage.FromJson(responseJson));
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            return LocalIpcResult.Failed($"IPC request to '{PipeName}' timed out.", timedOut: true);
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or LocalIpcException)
        {
            return LocalIpcResult.Failed(ex.Message);
        }
    }
}
