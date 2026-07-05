using RKWorkspace.Transport;
using RKWorkspace.Transport.Dev;
using RKWorkspace.Transport.NamedPipes;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransportHarness;

internal static class Program
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    private static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Contains("--help", StringComparer.OrdinalIgnoreCase))
            {
                PrintHelp();
                return 0;
            }

            var endpoint = GetOption(args, "--endpoint") ?? $"rkws-rkwp-dev-{Guid.NewGuid():N}";
            if (args.Contains("--server", StringComparer.OrdinalIgnoreCase))
            {
                return await RunServerAsync(endpoint).ConfigureAwait(false);
            }

            if (args.Contains("--client", StringComparer.OrdinalIgnoreCase))
            {
                return await RunClientAsync(endpoint).ConfigureAwait(false);
            }

            return await RunSmokeTestAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace RKWP Dev Transport Harness");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunSmokeTestAsync()
    {
        Console.WriteLine("RK Workspace RKWP Dev Transport Harness");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        Console.WriteLine("Transport: NamedPipeDev");

        var pipeName = $"rkws-rkwp-dev-smoke-{Guid.NewGuid():N}";
        var transport = CreateTransport();
        var endpoint = TransportEndpoint.NamedPipe(pipeName);
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        var sessionId = $"rkwp-session-{Guid.NewGuid():N}";

        await VerifyTimeoutAsync().ConfigureAwait(false);
        VerifyInvalidMessageCreation();
        await VerifyRawInvalidMessageAsync(pipeName).ConfigureAwait(false);

        using var serverCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await server.StartAsync(serverCancellation.Token).ConfigureAwait(false);
        Console.WriteLine("ServerStarted: OK");

        var serverTask = Task.Run(
            () => RunServerConversationAsync(server, sessionId, serverCancellation.Token),
            serverCancellation.Token);

        var helloResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                "ablage-windows-owner",
                "ablage-windows-guest",
                payload: new Dictionary<string, string>
                {
                    ["displayName"] = "Windows Owner"
                }),
            DefaultTimeout).ConfigureAwait(false);
        Ensure(helloResponse.MessageType == TransportMessageType.AblageCapabilities, "Hello did not return capabilities.");
        Ensure(helloResponse.SessionId == sessionId, "Hello did not negotiate session id.");
        Console.WriteLine("ClientConnected: OK");
        Console.WriteLine("AblageHello: OK");
        Console.WriteLine("SessionNegotiated: OK");

        var capabilities = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                "ablage-windows-owner",
                "ablage-windows-guest",
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameOnly"] = "true",
                    ["noFileIngress"] = "true"
                }),
            DefaultTimeout).ConfigureAwait(false);
        Ensure(capabilities.MessageType == TransportMessageType.AblageCapabilities, "Capabilities roundtrip failed.");
        Console.WriteLine("AblageCapabilities: OK");

        var heartbeat = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                "ablage-windows-owner",
                "ablage-windows-guest",
                sessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = "lease-smoke",
                    ["state"] = "alive"
                }),
            DefaultTimeout).ConfigureAwait(false);
        Ensure(heartbeat.MessageType == TransportMessageType.CarryLeaseHeartbeat, "Heartbeat roundtrip failed.");
        Console.WriteLine("CarryLeaseHeartbeat: OK");

        var frameUpdate = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                "ablage-windows-owner",
                "ablage-windows-guest",
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameId"] = "frame-rechnung",
                    ["page"] = "1",
                    ["mode"] = "FrameOnly"
                }),
            DefaultTimeout).ConfigureAwait(false);
        Ensure(frameUpdate.MessageType == TransportMessageType.FrameUpdate, "FrameUpdate roundtrip failed.");
        Console.WriteLine("FrameUpdate: OK");

        var error = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.ErrorResponse,
                "ablage-windows-owner",
                "ablage-windows-guest",
                sessionId,
                new Dictionary<string, string>
                {
                    ["error"] = "expected-smoke-error"
                }),
            DefaultTimeout).ConfigureAwait(false);
        Ensure(error.MessageType == TransportMessageType.ErrorResponse, "Error message roundtrip failed.");
        Console.WriteLine("ErrorMessage: OK");

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine("DisconnectDetected: OK");

        var diagnostics = transport.GetDiagnostics();
        Ensure(diagnostics.MessagesSent >= 5, "Diagnostics did not count sent messages.");
        Ensure(diagnostics.MessagesReceived >= 5, "Diagnostics did not count received messages.");
        Console.WriteLine("Diagnostics: OK");
        Console.WriteLine("SmokeTestNoHang: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task<int> RunServerAsync(string endpointName)
    {
        Console.WriteLine("RK Workspace RKWP Dev Transport Harness");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Mode: Server");
        Console.WriteLine($"Endpoint: {endpointName}");

        var transport = CreateTransport();
        var server = transport.CreateServer(TransportEndpoint.NamedPipe(endpointName));
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        Console.WriteLine("ServerStarted: OK");
        while (!cancellation.IsCancellationRequested)
        {
            var request = await server.WaitForMessageAsync(cancellation.Token).ConfigureAwait(false);
            await server.SendResponseAsync(CreateResponse(request, request.SessionId), cancellation.Token).ConfigureAwait(false);
            Console.WriteLine($"{request.MessageType}: OK");
        }

        return 0;
    }

    private static async Task<int> RunClientAsync(string endpointName)
    {
        Console.WriteLine("RK Workspace RKWP Dev Transport Harness");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Mode: Client");
        Console.WriteLine($"Endpoint: {endpointName}");

        var transport = CreateTransport();
        var client = transport.CreateClient(TransportEndpoint.NamedPipe(endpointName));
        var response = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                "ablage-dev-client",
                "ablage-dev-server"),
            DefaultTimeout).ConfigureAwait(false);
        Console.WriteLine($"Response: {response.MessageType}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task RunServerConversationAsync(
        IRkwpTransportServer server,
        string sessionId,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < 5; index++)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            await server.SendResponseAsync(CreateResponse(request, sessionId), cancellationToken).ConfigureAwait(false);
        }
    }

    private static RkwpTransportMessage CreateResponse(
        RkwpTransportMessage request,
        string sessionId)
    {
        return request.MessageType switch
        {
            TransportMessageType.AblageHello => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameOnly"] = "true",
                    ["heartbeat"] = "true",
                    ["noFileIngress"] = "true"
                },
                request.MessageId),
            TransportMessageType.ErrorResponse => RkwpTransportMessage.Create(
                TransportMessageType.ErrorResponse,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["error"] = request.Payload.TryGetValue("error", out var error)
                        ? error
                        : "expected-smoke-error"
                },
                request.MessageId),
            _ => RkwpTransportMessage.Create(
                request.MessageType,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                request.Payload,
                request.MessageId)
        };
    }

    private static async Task VerifyTimeoutAsync()
    {
        var transport = CreateTransport();
        var client = transport.CreateClient(TransportEndpoint.NamedPipe($"rkws-rkwp-missing-{Guid.NewGuid():N}"));
        try
        {
            await client.RequestAsync(
                RkwpTransportMessage.Create(
                    TransportMessageType.AblageHello,
                    "timeout-client",
                    "timeout-server"),
                TimeSpan.FromMilliseconds(220)).ConfigureAwait(false);
            throw new InvalidOperationException("Timeout check unexpectedly succeeded.");
        }
        catch (RkwpTransportException)
        {
            Console.WriteLine("TimeoutDetected: OK");
        }
    }

    private static void VerifyInvalidMessageCreation()
    {
        try
        {
            _ = RkwpTransportMessage.Create(
                TransportMessageType.Unknown,
                "invalid-client",
                "invalid-server");
            throw new InvalidOperationException("Invalid message check unexpectedly succeeded.");
        }
        catch (RkwpTransportException)
        {
            Console.WriteLine("InvalidMessageRejected: OK");
        }
    }

    private static async Task VerifyRawInvalidMessageAsync(string pipeName)
    {
        var transport = CreateTransport();
        var endpoint = TransportEndpoint.NamedPipe(pipeName);
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        var serverTask = Task.Run(async () =>
        {
            try
            {
                await server.WaitForMessageAsync(cancellation.Token).ConfigureAwait(false);
            }
            catch (RkwpTransportException)
            {
            }
        }, cancellation.Token);

        var result = await client.SendRawAsync("not-json", DefaultTimeout, cancellation.Token).ConfigureAwait(false);
        Ensure(!result.Success, "Raw invalid message unexpectedly succeeded.");
        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine("RawInvalidMessage: OK");
    }

    private static RkwpDevTransport CreateTransport()
    {
        return new RkwpDevTransport(new NamedPipeTransportOptions
        {
            DefaultTimeout = DefaultTimeout
        });
    }

    private static string? GetOption(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("RK Workspace RKWP Dev Transport Harness");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("--smoke-test              Runs the local RKWP transport smoke test.");
        Console.WriteLine("--server --endpoint NAME  Starts a NamedPipeDev server.");
        Console.WriteLine("--client --endpoint NAME  Sends AblageHello to a NamedPipeDev server.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
