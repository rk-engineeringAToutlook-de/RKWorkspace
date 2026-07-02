using System.Diagnostics;
using System.IO.Pipes;
using RKWorkspace.LocalIpc;

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
        var client = new LocalIpcClient($"rkws-missing-{Guid.NewGuid():N}");
        var result = await client.SendAsync(
            LocalIpcMessage.Create(
                LocalIpcMessageType.AgentStatusRequest,
                "harness",
                "rkws-agent-b"),
            TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);
        Ensure(!result.Success, "Server-not-reachable check unexpectedly succeeded.");
        Console.WriteLine("[OK] Server not reachable handled");
    }

    private static async Task VerifyInvalidMessageAsync(string pipeName)
    {
        var result = await new LocalIpcClient(pipeName)
            .SendRawAsync("not-json", IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Invalid-message check unexpectedly succeeded.");
        Console.WriteLine("[OK] Invalid message handled");
    }

    private static async Task VerifyUnknownMessageTypeAsync(string pipeName)
    {
        var raw = """
            {"messageId":"unknown-type","messageType":"DoesNotExist","sourceAgentId":"harness","targetAgentId":"rkws-agent-b","timestamp":"2026-07-02T00:00:00+00:00","payload":{}}
            """;
        var result = await new LocalIpcClient(pipeName)
            .SendRawAsync(raw, IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Unknown-message-type check unexpectedly succeeded.");
        Console.WriteLine("[OK] Unknown MessageType handled");
    }

    private static async Task VerifyWrongTargetAsync(string pipeName)
    {
        var result = await new LocalIpcClient(pipeName)
            .SendAsync(
                LocalIpcMessage.Create(
                    LocalIpcMessageType.AgentStatusRequest,
                    "harness",
                    "wrong-agent"),
                IpcTimeout)
            .ConfigureAwait(false);
        Ensure(!result.Success, "Wrong-target check unexpectedly succeeded.");
        Console.WriteLine("[OK] Wrong TargetAgentId handled");
    }

    private static async Task VerifyTransferWithoutPayloadAsync(string pipeName)
    {
        var result = await new LocalIpcClient(pipeName)
            .SendAsync(
                LocalIpcMessage.Create(
                    LocalIpcMessageType.TransferRequest,
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
        using var serverReady = new ManualResetEventSlim(initialState: false);
        using var serverCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var serverTask = Task.Run(async () =>
        {
            await using var server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            serverReady.Set();
            await server.WaitForConnectionAsync(serverCancellation.Token).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(2), serverCancellation.Token).ConfigureAwait(false);
        }, serverCancellation.Token);

        serverReady.Wait(TimeSpan.FromSeconds(1));
        var result = await new LocalIpcClient(pipeName)
            .SendAsync(
                LocalIpcMessage.Create(
                    LocalIpcMessageType.AgentStatusRequest,
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
        catch (IOException)
        {
        }

        Console.WriteLine("[OK] Timeout handled");
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
