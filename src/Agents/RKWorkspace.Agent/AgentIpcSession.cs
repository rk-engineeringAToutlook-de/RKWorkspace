using RKWorkspace.Core.TransferObjects;
using RKWorkspace.LocalIpc;

namespace RKWorkspace.Agent;

internal sealed class AgentIpcSession
{
    private const string DefaultTargetAgentId = "rkws-agent-b";

    private readonly AgentRuntime _agent;
    private readonly TextWriter _output;

    public AgentIpcSession(AgentRuntime agent, TextWriter? output = null)
    {
        _agent = agent;
        _output = output ?? Console.Out;
    }

    public async Task<int> RunServerAsync(
        string pipeName,
        bool stopAfterTransfer,
        CancellationToken cancellationToken = default)
    {
        using var consoleCancellation = new CancellationTokenSource();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            consoleCancellation.Token,
            cancellationToken);
        ConsoleCancelEventHandler handler = (_, args) =>
        {
            args.Cancel = true;
            consoleCancellation.Cancel();
        };

        Console.CancelKeyPress += handler;
        try
        {
            _agent.Start();
            _output.WriteLine($"IPC server ready: {pipeName}");

            var server = new LocalIpcServer(pipeName);
            await server.RunAsync(
                HandleMessageAsync,
                response => ShouldStopServer(response, stopAfterTransfer),
                linked.Token).ConfigureAwait(false);
            _agent.Stop();
            return 0;
        }
        catch (OperationCanceledException)
        {
            _agent.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"RK Workspace Agent failed: {ex.Message}");
            TryStopAgent();
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    public async Task<int> RunClientAsync(
        string pipeName,
        string? targetAgentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _agent.Start();
            var target = string.IsNullOrWhiteSpace(targetAgentId)
                ? DefaultTargetAgentId
                : targetAgentId;
            var client = new LocalIpcClient(pipeName);

            var hello = await SendAsync(
                client,
                LocalIpcMessage.Create(
                    LocalIpcMessageType.AgentHello,
                    _agent.Configuration.AgentId,
                    target,
                    BasePayload()),
                "AgentHello",
                cancellationToken).ConfigureAwait(false);

            var status = await SendAsync(
                client,
                LocalIpcMessage.Create(
                    LocalIpcMessageType.AgentStatusRequest,
                    _agent.Configuration.AgentId,
                    target,
                    BasePayload()),
                "StatusRequest",
                cancellationToken).ConfigureAwait(false);

            var transferObject = CreateTransferObject();
            var transfer = await SendAsync(
                client,
                LocalIpcMessage.Create(
                    LocalIpcMessageType.TransferRequest,
                    _agent.Configuration.AgentId,
                    target,
                    TransferPayload(transferObject)),
                "TransferRequest",
                cancellationToken).ConfigureAwait(false);

            if (!hello.Success || !status.Success || !transfer.Success)
            {
                _agent.Stop();
                return 1;
            }

            var transferSucceeded = transfer.Response?.Payload.TryGetValue("success", out var successText) == true &&
                string.Equals(successText, "true", StringComparison.OrdinalIgnoreCase);
            _output.WriteLine($"TransferResponse: {(transferSucceeded ? "SUCCESS" : "FAILED")}");
            _agent.Stop();
            return transferSucceeded ? 0 : 1;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"RK Workspace Agent failed: {ex.Message}");
            TryStopAgent();
            return 1;
        }
    }

    private Task<LocalIpcMessage> HandleMessageAsync(
        LocalIpcMessage message,
        CancellationToken cancellationToken)
    {
        if (!IsMessageForThisAgent(message))
        {
            return Task.FromResult(ErrorToSource(
                message,
                $"TargetAgentId '{message.TargetAgentId}' does not match '{_agent.Configuration.AgentId}'."));
        }

        return Task.FromResult(message.MessageType switch
        {
            LocalIpcMessageType.AgentHello => Response(
                LocalIpcMessageType.AgentHello,
                message,
                StatusPayload("OK")),
            LocalIpcMessageType.AgentStatusRequest => Response(
                LocalIpcMessageType.AgentStatusResponse,
                message,
                StatusPayload("OK")),
            LocalIpcMessageType.WorkspaceAdvertisement => Response(
                LocalIpcMessageType.WorkspaceAdvertisement,
                message,
                StatusPayload("OK")),
            LocalIpcMessageType.TransferRequest => HandleTransferRequest(message),
            LocalIpcMessageType.ShutdownRequest => Response(
                LocalIpcMessageType.AgentStatusResponse,
                message,
                ShutdownPayload()),
            _ => ErrorToSource(message, $"Unsupported IPC message type '{message.MessageType}'.")
        });
    }

    private LocalIpcMessage HandleTransferRequest(LocalIpcMessage message)
    {
        var required = new[] { "requestId", "transferObjectId", "sourceWorkspaceId" };
        var missing = required
            .Where(key => !message.Payload.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            .ToArray();
        if (missing.Length > 0)
        {
            return ErrorToSource(message, $"TransferRequest payload is missing: {string.Join(", ", missing)}.");
        }

        var targetWorkspace = _agent.LocalWorkspaceId?.ToString() ?? string.Empty;
        return Response(
            LocalIpcMessageType.TransferResponse,
            message,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["success"] = "true",
                ["requestId"] = message.Payload["requestId"],
                ["sourceWorkspaceId"] = message.Payload["sourceWorkspaceId"],
                ["targetWorkspaceId"] = targetWorkspace,
                ["transferObjectId"] = message.Payload["transferObjectId"],
                ["finalState"] = TransferObjectState.Completed.ToString(),
                ["message"] = "Local IPC transfer accepted."
            });
    }

    private async Task<LocalIpcResult> SendAsync(
        LocalIpcClient client,
        LocalIpcMessage message,
        string label,
        CancellationToken cancellationToken)
    {
        var result = await client.SendAsync(
            message,
            TimeSpan.FromSeconds(5),
            cancellationToken).ConfigureAwait(false);
        if (result.Success)
        {
            _output.WriteLine($"{label}: OK");
            return result;
        }

        _output.WriteLine($"{label}: FAILED - {result.Error}");
        return result;
    }

    private ITransferObject CreateTransferObject()
    {
        var now = DateTimeOffset.UtcNow;
        return _agent.Runtime.TransferObjectManager.Create(
            TransferObjectType.Text,
            new TransferMetadata
            {
                ObjectId = TransferObjectId.NewId(),
                DisplayName = "Local IPC Text",
                MimeType = "text/plain; charset=utf-8",
                Size = "Local IPC Text".Length,
                Checksum = "sha256:local-ipc-text",
                CreatedAt = now,
                ModifiedAt = now,
                SourceWorkspace = _agent.LocalWorkspaceId?.ToString() ?? string.Empty,
                TargetWorkspace = string.Empty,
                Owner = _agent.Configuration.AgentId,
                Priority = 1,
                Tags = new[] { "local-ipc", "two-process", "text" },
                Version = "1.0.0"
            });
    }

    private IReadOnlyDictionary<string, string> BasePayload()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["workspaceId"] = _agent.LocalWorkspaceId?.ToString() ?? string.Empty,
            ["workspaceName"] = _agent.Configuration.WorkspaceName,
            ["runtimeState"] = _agent.GetDiagnostics().RuntimeState.ToString()
        };
    }

    private IReadOnlyDictionary<string, string> TransferPayload(ITransferObject transferObject)
    {
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requestId"] = $"local-ipc-request-{Guid.NewGuid():N}",
            ["transferObjectId"] = transferObject.Id.ToString(),
            ["sourceWorkspaceId"] = _agent.LocalWorkspaceId?.ToString() ?? string.Empty,
            ["sourceWorkspaceName"] = _agent.Configuration.WorkspaceName,
            ["objectType"] = transferObject.ObjectType.ToString(),
            ["direction"] = "Right"
        };

        return payload;
    }

    private Dictionary<string, string> StatusPayload(string status)
    {
        var diagnostics = _agent.GetDiagnostics();
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["status"] = status,
            ["agentId"] = diagnostics.AgentId,
            ["workspaceId"] = diagnostics.WorkspaceId,
            ["workspaceName"] = diagnostics.WorkspaceName,
            ["runtimeState"] = diagnostics.RuntimeState.ToString(),
            ["agentState"] = diagnostics.State.ToString()
        };
    }

    private Dictionary<string, string> ShutdownPayload()
    {
        var payload = StatusPayload("OK");
        payload["shutdown"] = "true";
        return payload;
    }

    private LocalIpcMessage Response(
        LocalIpcMessageType type,
        LocalIpcMessage request,
        IReadOnlyDictionary<string, string> payload)
    {
        return LocalIpcMessage.Create(
            type,
            _agent.Configuration.AgentId,
            request.SourceAgentId,
            payload);
    }

    private LocalIpcMessage ErrorToSource(LocalIpcMessage request, string error)
    {
        return LocalIpcServer.Error(
            _agent.Configuration.AgentId,
            request.SourceAgentId,
            error);
    }

    private bool IsMessageForThisAgent(LocalIpcMessage message)
    {
        return string.Equals(message.TargetAgentId, _agent.Configuration.AgentId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(message.TargetAgentId, "*", StringComparison.Ordinal);
    }

    private static bool ShouldStopServer(LocalIpcMessage response, bool stopAfterTransfer)
    {
        if (response.Payload.TryGetValue("shutdown", out var shutdown) &&
            string.Equals(shutdown, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return stopAfterTransfer &&
            response.MessageType == LocalIpcMessageType.TransferResponse &&
            response.Payload.TryGetValue("success", out var success) &&
            string.Equals(success, "true", StringComparison.OrdinalIgnoreCase);
    }

    private void TryStopAgent()
    {
        try
        {
            _agent.Stop();
        }
        catch
        {
            // Best-effort cleanup after an already reported failure.
        }
    }
}
