using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Transport;
using RKWorkspace.Transport.NamedPipes;

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

            ITransport transport = CreateLocalTransport();
            await transport.StartAsync(linked.Token).ConfigureAwait(false);
            ITransportServer server = transport.CreateServer(TransportEndpoint.NamedPipe(pipeName));
            await RunServerLoopAsync(
                server,
                HandleMessageAsync,
                response => ShouldStopServer(response, stopAfterTransfer),
                linked.Token).ConfigureAwait(false);
            await transport.StopAsync(CancellationToken.None).ConfigureAwait(false);
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
            ITransport transport = CreateLocalTransport();
            await transport.StartAsync(cancellationToken).ConfigureAwait(false);
            ITransportClient client = transport.CreateClient(TransportEndpoint.NamedPipe(pipeName));

            var hello = await SendAsync(
                client,
                TransportMessage.Create(
                    TransportMessageType.AgentHello,
                    _agent.Configuration.AgentId,
                    target,
                    BasePayload()),
                "AgentHello",
                cancellationToken).ConfigureAwait(false);

            var status = await SendAsync(
                client,
                TransportMessage.Create(
                    TransportMessageType.AgentStatusRequest,
                    _agent.Configuration.AgentId,
                    target,
                    BasePayload()),
                "StatusRequest",
                cancellationToken).ConfigureAwait(false);

            var transferObject = CreateTransferObject();
            var transfer = await SendAsync(
                client,
                TransportMessage.Create(
                    TransportMessageType.TransferRequest,
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

            var transferSucceeded = transfer.Message?.Payload.TryGetValue("success", out var successText) == true &&
                string.Equals(successText, "true", StringComparison.OrdinalIgnoreCase);
            _output.WriteLine($"TransferResponse: {(transferSucceeded ? "SUCCESS" : "FAILED")}");
            await transport.StopAsync(CancellationToken.None).ConfigureAwait(false);
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

    private async Task RunServerLoopAsync(
        ITransportServer server,
        Func<TransportMessage, CancellationToken, Task<TransportMessage>> handler,
        Func<TransportMessage, bool>? stopAfterResponse,
        CancellationToken cancellationToken)
    {
        await server.StartAsync(cancellationToken).ConfigureAwait(false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            var response = await HandleTransportRequestAsync(request, handler, cancellationToken).ConfigureAwait(false);
            await server.SendResponseAsync(response, cancellationToken).ConfigureAwait(false);

            if (stopAfterResponse?.Invoke(response) == true)
            {
                await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
                return;
            }
        }
    }

    private static async Task<TransportMessage> HandleTransportRequestAsync(
        TransportResult request,
        Func<TransportMessage, CancellationToken, Task<TransportMessage>> handler,
        CancellationToken cancellationToken)
    {
        if (!request.Success || request.Message is null)
        {
            return Error("local-ipc-server", "unknown", request.Error);
        }

        return await handler(request.Message, cancellationToken).ConfigureAwait(false);
    }

    private Task<TransportMessage> HandleMessageAsync(
        TransportMessage message,
        CancellationToken cancellationToken)
    {
        if (!IsMessageForThisAgent(message))
        {
            return Task.FromResult(ErrorToSource(
                message,
                $"TargetId '{message.TargetId}' does not match '{_agent.Configuration.AgentId}'."));
        }

        return Task.FromResult(message.MessageType switch
        {
            TransportMessageType.AgentHello => Response(
                TransportMessageType.AgentHello,
                message,
                StatusPayload("OK")),
            TransportMessageType.AgentStatusRequest => Response(
                TransportMessageType.AgentStatusResponse,
                message,
                StatusPayload("OK")),
            TransportMessageType.WorkspaceAdvertisement => Response(
                TransportMessageType.WorkspaceAdvertisement,
                message,
                StatusPayload("OK")),
            TransportMessageType.TransferRequest => HandleTransferRequest(message),
            TransportMessageType.ShutdownRequest => Response(
                TransportMessageType.AgentStatusResponse,
                message,
                ShutdownPayload()),
            _ => ErrorToSource(message, $"Unsupported IPC message type '{message.MessageType}'.")
        });
    }

    private TransportMessage HandleTransferRequest(TransportMessage message)
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
            TransportMessageType.TransferResponse,
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

    private async Task<TransportResult> SendAsync(
        ITransportClient client,
        TransportMessage message,
        string label,
        CancellationToken cancellationToken)
    {
        var result = await client.RequestAsync(
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

    private TransportMessage Response(
        TransportMessageType type,
        TransportMessage request,
        IReadOnlyDictionary<string, string> payload)
    {
        return TransportMessage.Create(
            type,
            _agent.Configuration.AgentId,
            request.SourceId,
            payload,
            correlationId: request.MessageId);
    }

    private TransportMessage ErrorToSource(TransportMessage request, string error)
    {
        return Error(
            _agent.Configuration.AgentId,
            request.SourceId,
            error);
    }

    private bool IsMessageForThisAgent(TransportMessage message)
    {
        return string.Equals(message.TargetId, _agent.Configuration.AgentId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(message.TargetId, "*", StringComparison.Ordinal);
    }

    private static bool ShouldStopServer(TransportMessage response, bool stopAfterTransfer)
    {
        if (response.Payload.TryGetValue("shutdown", out var shutdown) &&
            string.Equals(shutdown, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return stopAfterTransfer &&
            response.MessageType == TransportMessageType.TransferResponse &&
            response.Payload.TryGetValue("success", out var success) &&
            string.Equals(success, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static TransportMessage Error(string sourceId, string targetId, string message)
    {
        return TransportMessage.Create(
            TransportMessageType.ErrorResponse,
            sourceId,
            targetId,
            new Dictionary<string, string>
            {
                ["error"] = string.IsNullOrWhiteSpace(message) ? "Transport error." : message
            });
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

    private static ITransport CreateLocalTransport()
    {
        return new NamedPipeTransport();
    }
}
