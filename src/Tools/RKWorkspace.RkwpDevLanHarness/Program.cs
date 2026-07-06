using RKWorkspace.RkwpTransport.DevLan;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpDevLanHarness;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = DevLanHarnessOptions.Parse(args);
            if (options.Mode == DevLanHarnessMode.Owner)
            {
                return await RunOwnerAsync(options).ConfigureAwait(false);
            }

            if (options.Mode == DevLanHarnessMode.Guest)
            {
                return await RunGuestAsync(options).ConfigureAwait(false);
            }

            return await RunSmokeAsync(options).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace RKWP DevLan Harness");
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunSmokeAsync(DevLanHarnessOptions options)
    {
        var sessionId = string.IsNullOrWhiteSpace(options.SessionId)
            ? $"rkwp-devlan-smoke-{Guid.NewGuid():N}"
            : options.SessionId;
        var transport = new RkwpDevLanTransport(new RkwpDevLanOptions
        {
            BindAddress = options.BindAddress,
            Host = options.Host,
            Port = options.Port,
            SessionId = sessionId,
            DevPairingAllowed = true,
            SecureSessionRequired = false,
            DefaultTimeout = TimeSpan.FromSeconds(3)
        });
        var endpoint = RkwpDevLanEndpoint.Create(options.BindAddress, options.Port);
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(RkwpDevLanEndpoint.Create(options.Host, options.Port));

        Console.WriteLine("RK Workspace RKWP DevLan Smoke");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        Console.WriteLine("Transport: LocalNetworkDev");
        Console.WriteLine($"BindAddress: {options.BindAddress}");
        Console.WriteLine($"Port: {options.Port}");
        Console.WriteLine("SecurityMode: DevelopmentInsecure");
        Console.WriteLine("DevSecurityWarning: OK - Lab only, no final TLS.");

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        Console.WriteLine("OwnerServerStarted: OK");

        var serverTask = Task.Run(
            () => RunServerConversationAsync(server, sessionId, expectedMessages: 4, cancellation.Token),
            cancellation.Token);

        var hello = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                "ablage-devlan-guest",
                "ablage-devlan-owner",
                payload: new Dictionary<string, string>
                {
                    ["displayName"] = "Ablage DevLan Guest",
                    ["devIdentity"] = "guest-devlan-smoke"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(hello.MessageType == TransportMessageType.AblageCapabilities, "AblageHello failed.");
        Ensure(hello.SessionId == sessionId, "Session id was not returned.");
        Console.WriteLine("GuestClientConnected: OK");
        Console.WriteLine("AblageHello: OK");

        var capabilities = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                "ablage-devlan-guest",
                "ablage-devlan-owner",
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameOnly"] = "true",
                    ["noFileIngress"] = "true",
                    ["devPairing"] = "requested"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(capabilities.MessageType == TransportMessageType.AblageCapabilities, "Capabilities failed.");
        Ensure(capabilities.Payload.TryGetValue("devPairing", out var devPairing) && devPairing == "allowed", "DevPairing failed.");
        Console.WriteLine("AblageCapabilities: OK");
        Console.WriteLine("DevPairing: OK");
        Console.WriteLine("SecureSessionSpike: PREPARED");

        var heartbeat = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                "ablage-devlan-guest",
                "ablage-devlan-owner",
                sessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = "lease-devlan-smoke",
                    ["state"] = "alive"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(heartbeat.MessageType == TransportMessageType.CarryLeaseHeartbeat, "Heartbeat failed.");
        Console.WriteLine("Heartbeat: OK");

        var frameUpdate = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                "ablage-devlan-owner",
                "ablage-devlan-guest",
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameId"] = "frame-devlan-smoke",
                    ["containsOriginalFileBytes"] = "false",
                    ["hasOriginalPath"] = "false"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(frameUpdate.MessageType == TransportMessageType.FrameUpdate, "FrameUpdate failed.");
        Ensure(frameUpdate.Payload.TryGetValue("noFileIngress", out var noFileIngress) && noFileIngress == "true", "NoFileIngress failed.");
        Console.WriteLine("FrameUpdate: OK");

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine("Disconnect: OK");

        var diagnostics = transport.GetDiagnostics();
        Ensure(diagnostics.MessagesSent >= 4, "Diagnostics did not count sent messages.");
        Ensure(diagnostics.MessagesReceived >= 4, "Diagnostics did not count received messages.");
        Console.WriteLine("Diagnostics: OK");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task<int> RunOwnerAsync(DevLanHarnessOptions options)
    {
        var sessionId = string.IsNullOrWhiteSpace(options.SessionId)
            ? $"rkwp-devlan-owner-{Guid.NewGuid():N}"
            : options.SessionId;
        var transport = new RkwpDevLanTransport(new RkwpDevLanOptions
        {
            BindAddress = options.BindAddress,
            Host = options.Host,
            Port = options.Port,
            SessionId = sessionId,
            DevPairingAllowed = options.AllowDevPairing
        });
        var server = transport.CreateServer(RkwpDevLanEndpoint.Create(options.BindAddress, options.Port));

        Console.WriteLine("RK Workspace RKWP DevLan Owner");
        Console.WriteLine("------------------------------");
        Console.WriteLine($"URL: rkwp+tcp-dev://{options.BindAddress}:{options.Port}");
        Console.WriteLine("SecurityMode: DevelopmentInsecure");
        Console.WriteLine("DevSecurityWarning: Lab only, no final TLS.");

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        Console.WriteLine("OwnerServerStarted: OK");

        while (!cancellation.IsCancellationRequested)
        {
            var request = await server.WaitForMessageAsync(cancellation.Token).ConfigureAwait(false);
            await server.SendResponseAsync(CreateResponse(request, sessionId), cancellation.Token).ConfigureAwait(false);
            Console.WriteLine($"{request.MessageType}: OK");
        }

        return 0;
    }

    private static async Task<int> RunGuestAsync(DevLanHarnessOptions options)
    {
        var transport = new RkwpDevLanTransport(new RkwpDevLanOptions
        {
            Host = options.Host,
            Port = options.Port,
            DefaultTimeout = TimeSpan.FromSeconds(5)
        });
        var client = transport.CreateClient(RkwpDevLanEndpoint.Create(options.Host, options.Port));

        Console.WriteLine("RK Workspace RKWP DevLan Guest");
        Console.WriteLine("------------------------------");
        Console.WriteLine($"ConnectTo: rkwp+tcp-dev://{options.Host}:{options.Port}");
        var response = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                "ablage-devlan-guest",
                "ablage-devlan-owner",
                payload: new Dictionary<string, string>
                {
                    ["displayName"] = "Ablage DevLan Guest"
                }),
            TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Console.WriteLine($"Response: {response.MessageType}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task RunServerConversationAsync(
        IRkwpTransportServer server,
        string sessionId,
        int expectedMessages,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < expectedMessages; index++)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            await server.SendResponseAsync(CreateResponse(request, sessionId), cancellationToken).ConfigureAwait(false);
        }
    }

    private static RkwpTransportMessage CreateResponse(RkwpTransportMessage request, string sessionId)
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
                    ["noFileIngress"] = "true",
                    ["devPairing"] = "allowed",
                    ["secureSessionSpike"] = "prepared"
                },
                request.MessageId),
            TransportMessageType.AblageCapabilities => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["devPairing"] = "allowed",
                    ["noFileIngress"] = "true"
                },
                request.MessageId),
            TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frame"] = "accepted",
                    ["noFileIngress"] = "true"
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

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal enum DevLanHarnessMode
{
    SmokeTest,
    Owner,
    Guest
}

internal sealed record DevLanHarnessOptions(
    DevLanHarnessMode Mode,
    string BindAddress,
    string Host,
    int Port,
    string SessionId,
    bool AllowDevPairing)
{
    public static DevLanHarnessOptions Parse(string[] args)
    {
        var mode = DevLanHarnessMode.SmokeTest;
        var bindAddress = "127.0.0.1";
        var host = "127.0.0.1";
        var port = 57100;
        var sessionId = string.Empty;
        var allowDevPairing = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--owner", "-Owner"))
            {
                mode = DevLanHarnessMode.Owner;
                continue;
            }

            if (Is(arg, "--guest", "-Guest"))
            {
                mode = DevLanHarnessMode.Guest;
                continue;
            }

            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                mode = DevLanHarnessMode.SmokeTest;
                continue;
            }

            if (Is(arg, "--allow-dev-pairing", "-AllowDevPairing"))
            {
                allowDevPairing = true;
                continue;
            }

            if (Is(arg, "--bind-address", "-BindAddress") && index + 1 < args.Length)
            {
                bindAddress = args[++index];
                continue;
            }

            if (Is(arg, "--host", "-Host") && index + 1 < args.Length)
            {
                host = args[++index];
                continue;
            }

            if (Is(arg, "--port", "-Port") && index + 1 < args.Length && int.TryParse(args[index + 1], out var parsedPort))
            {
                port = parsedPort;
                index++;
                continue;
            }

            if (Is(arg, "--session-id", "-SessionId") && index + 1 < args.Length)
            {
                sessionId = args[++index];
            }
        }

        return new DevLanHarnessOptions(mode, bindAddress, host, port, sessionId, allowDevPairing);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
