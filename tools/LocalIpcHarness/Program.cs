using System.Diagnostics;
using RKWorkspace.Transport;
using RKWorkspace.Transport.NamedPipes;

namespace RKWorkspace.LocalIpcHarness;

internal static class Program
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan IpcTimeout = TimeSpan.FromSeconds(2);

    private static async Task<int> Main()
    {
        Console.WriteLine("RK Workspace Local IPC Harness");
        Console.WriteLine("------------------------------");

        ManagedProcess? agentB = null;
        try
        {
            var root = Directory.GetCurrentDirectory();
            var agentProject = Path.Combine(
                root,
                "src",
                "Agents",
                "RKWorkspace.Agent",
                "RKWorkspace.Agent.csproj");
            var pipeName = $"rkws-local-ipc-{Guid.NewGuid():N}";

            await VerifyServerNotReachableAsync().ConfigureAwait(false);

            agentB = StartAgent(
                root,
                agentProject,
                "--agent-id", "rkws-agent-b",
                "--workspace-name", "Workspace-B",
                "--position", "Right",
                "--ipc-server", pipeName,
                "--ipc-stop-after-transfer");
            await agentB.WaitForOutputAsync("IPC server ready:", ProcessTimeout).ConfigureAwait(false);
            Console.WriteLine("Agent B started");

            await VerifyInvalidMessageAsync(pipeName).ConfigureAwait(false);
            await VerifyUnknownMessageTypeAsync(pipeName).ConfigureAwait(false);
            await VerifyWrongTargetAsync(pipeName).ConfigureAwait(false);
            await VerifyTransferWithoutPayloadAsync(pipeName).ConfigureAwait(false);
            await VerifyTimeoutAsync().ConfigureAwait(false);

            var agentA = StartAgent(
                root,
                agentProject,
                "--agent-id", "rkws-agent-a",
                "--workspace-name", "Workspace-A",
                "--position", "Left",
                "--ipc-client", pipeName,
                "--target-agent-id", "rkws-agent-b",
                "--once");
            Console.WriteLine("Agent A started");
            await agentA.WaitForExitAsync(ProcessTimeout).ConfigureAwait(false);
            Ensure(agentA.ExitCode == 0, $"Agent A failed with exit code {agentA.ExitCode}.");
            EnsureOutput(agentA, "AgentHello: OK");
            EnsureOutput(agentA, "StatusRequest: OK");
            EnsureOutput(agentA, "TransferRequest: OK");
            EnsureOutput(agentA, "TransferResponse: SUCCESS");
            Console.WriteLine("AgentHello: OK");
            Console.WriteLine("StatusRequest: OK");
            Console.WriteLine("TransferRequest: OK");
            Console.WriteLine("TransferResponse: SUCCESS");
            Console.WriteLine("Agent A stopped");

            await agentB.WaitForExitAsync(ProcessTimeout).ConfigureAwait(false);
            Ensure(agentB.ExitCode == 0, $"Agent B failed with exit code {agentB.ExitCode}.");
            Console.WriteLine("Agent B stopped");
            Console.WriteLine("RESULT: SUCCESS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            agentB?.KillIfRunning();
            return 1;
        }
    }

    private static ManagedProcess StartAgent(
        string root,
        string agentProject,
        params string[] agentArgs)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(agentProject);
        startInfo.ArgumentList.Add("--");
        foreach (var arg in agentArgs)
        {
            startInfo.ArgumentList.Add(arg);
        }

        return ManagedProcess.Start(startInfo);
    }

    private static async Task VerifyServerNotReachableAsync()
    {
        var client = CreateClient($"rkws-missing-{Guid.NewGuid():N}");
        var result = await client.RequestAsync(
            TransportMessage.Create(
                TransportMessageType.AgentStatusRequest,
                "harness",
                "rkws-agent-b"),
            TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);
        Ensure(!result.Success, "Server-not-reachable check unexpectedly succeeded.");
        Console.WriteLine("[OK] Server not reachable handled");
    }

    private static async Task VerifyInvalidMessageAsync(string pipeName)
    {
        var result = await CreateRawClient(pipeName)
            .SendRawAsync("not-json", IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Invalid-message check unexpectedly succeeded.");
        Console.WriteLine("[OK] Invalid message handled");
    }

    private static async Task VerifyUnknownMessageTypeAsync(string pipeName)
    {
        var raw = """
            {"messageId":"unknown-type","messageType":"DoesNotExist","sourceId":"harness","targetId":"rkws-agent-b","timestamp":"2026-07-02T00:00:00+00:00","payload":{}}
            """;
        var result = await CreateRawClient(pipeName)
            .SendRawAsync(raw, IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Unknown-message-type check unexpectedly succeeded.");
        Console.WriteLine("[OK] Unknown MessageType handled");
    }

    private static async Task VerifyWrongTargetAsync(string pipeName)
    {
        var result = await CreateClient(pipeName)
            .RequestAsync(
                TransportMessage.Create(
                    TransportMessageType.AgentStatusRequest,
                    "harness",
                    "wrong-agent"),
                IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Wrong-target check unexpectedly succeeded.");
        Console.WriteLine("[OK] Wrong TargetAgentId handled");
    }

    private static async Task VerifyTransferWithoutPayloadAsync(string pipeName)
    {
        var result = await CreateClient(pipeName)
            .RequestAsync(
                TransportMessage.Create(
                    TransportMessageType.TransferRequest,
                    "harness",
                    "rkws-agent-b"),
                IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Transfer-without-payload check unexpectedly succeeded.");
        Console.WriteLine("[OK] TransferRequest without payload handled");
    }

    private static async Task VerifyTimeoutAsync()
    {
        var pipeName = $"rkws-timeout-{Guid.NewGuid():N}";
        using var serverCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        ITransport transport = CreateLocalTransport();
        ITransportServer server = transport.CreateServer(TransportEndpoint.NamedPipe(pipeName));
        await server.StartAsync(serverCancellation.Token).ConfigureAwait(false);
        var serverTask = Task.Run(async () =>
        {
            var request = await server.WaitForMessageAsync(serverCancellation.Token).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(2), serverCancellation.Token).ConfigureAwait(false);
            if (request.Message is not null)
            {
                await server.SendResponseAsync(
                    TransportMessage.Create(
                        TransportMessageType.AgentStatusResponse,
                        "timeout-server",
                        request.Message.SourceId,
                        correlationId: request.Message.MessageId),
                    serverCancellation.Token).ConfigureAwait(false);
            }
        }, serverCancellation.Token);

        var result = await CreateClient(pipeName)
            .RequestAsync(
                TransportMessage.Create(
                    TransportMessageType.AgentStatusRequest,
                    "harness",
                    "rkws-agent-b"),
                TimeSpan.FromMilliseconds(200))
            .ConfigureAwait(false);
        Ensure(!result.Success && result.TimedOut, "Timeout check did not time out as expected.");
        serverCancellation.Cancel();
        try
        {
            await serverTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (TransportException)
        {
        }

        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine("[OK] Timeout handled");
    }

    private static ITransportClient CreateClient(string pipeName)
    {
        return CreateLocalTransport().CreateClient(TransportEndpoint.NamedPipe(pipeName));
    }

    private static NamedPipeTransportClient CreateRawClient(string pipeName)
    {
        return (NamedPipeTransportClient)CreateLocalTransport()
            .CreateClient(TransportEndpoint.NamedPipe(pipeName));
    }

    private static ITransport CreateLocalTransport()
    {
        return new NamedPipeTransport(
            new NamedPipeTransportOptions
            {
                DefaultTimeout = IpcTimeout
            });
    }

    private static void EnsureOutput(ManagedProcess process, string expected)
    {
        Ensure(
            process.Output.Any(line => line.Contains(expected, StringComparison.Ordinal)),
            $"Output did not contain '{expected}'.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
