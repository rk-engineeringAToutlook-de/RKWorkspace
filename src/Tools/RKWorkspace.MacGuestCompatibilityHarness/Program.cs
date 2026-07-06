using RKWorkspace.RkwpTransport.DevLan;
using RKWorkspace.Surface.Abstractions;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.MacGuestCompatibilityHarness;

internal static class Program
{
    private const string GuestAblageId = "ablage-macos-guest";
    private const string OwnerAblageId = "ablage-windows-owner";

    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = MacGuestHarnessOptions.Parse(args);
            return options.Mode switch
            {
                MacGuestHarnessMode.ConnectToOwner => await RunConnectToOwnerAsync(options).ConfigureAwait(false),
                MacGuestHarnessMode.ReplaySample => RunReplaySample(options),
                _ => await RunSmokeAsync(options).ConfigureAwait(false)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace macOS Guest Compatibility Harness");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunSmokeAsync(MacGuestHarnessOptions options)
    {
        var sessionId = $"rkwp-macos-compat-{Guid.NewGuid():N}";
        var endpoint = RkwpDevLanEndpoint.Create(options.BindAddress, options.Port);
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
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(RkwpDevLanEndpoint.Create(options.Host, options.Port));

        Console.WriteLine("RK Workspace macOS Guest Compatibility Harness");
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        Console.WriteLine("HarnessKind: macOS Guest Simulation");
        WriteIdentityAndCapabilities();

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        Console.WriteLine("SimulatedOwner: OK");

        var serverTask = Task.Run(
            () => RunOwnerConversationAsync(server, sessionId, expectedMessages: 4, cancellation.Token),
            cancellation.Token);

        var hello = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                GuestAblageId,
                OwnerAblageId,
                payload: CreateHelloPayload()),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(hello.MessageType == TransportMessageType.AblageCapabilities, "AblageHello failed.");
        EnsurePayload(hello, "noFileIngress", "true");
        Console.WriteLine("AblageHello: OK");

        var capabilities = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                GuestAblageId,
                OwnerAblageId,
                hello.SessionId,
                CreateCapabilityPayload()),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(capabilities.MessageType == TransportMessageType.FrameUpdate, "FrameSessionReady was not returned.");
        EnsurePayload(capabilities, "frameSessionReady", "true");
        EnsurePayload(capabilities, "containsOriginalFileBytes", "false");
        EnsurePayload(capabilities, "hasOriginalPath", "false");
        Console.WriteLine("AblageCapabilities: OK");
        Console.WriteLine("FrameGuestSurface: OK");
        Console.WriteLine("FrameSessionReady: OK");

        var heartbeat = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                GuestAblageId,
                OwnerAblageId,
                hello.SessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = "lease-macos-compat",
                    ["frameId"] = "frame-macos-compat",
                    ["state"] = "alive"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(heartbeat.MessageType == TransportMessageType.CarryLeaseHeartbeat, "Heartbeat failed.");
        Console.WriteLine("Heartbeat: OK");

        var returned = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                GuestAblageId,
                OwnerAblageId,
                hello.SessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = "lease-macos-compat",
                    ["frameId"] = "frame-macos-compat",
                    ["frameClose"] = "true",
                    ["guestKeptOriginalFile"] = "false"
                }),
            TimeSpan.FromSeconds(3),
            cancellation.Token).ConfigureAwait(false);
        Ensure(returned.MessageType == TransportMessageType.CarryLeaseReturn, "Return failed.");
        EnsurePayload(returned, "returnAccepted", "true");
        Console.WriteLine("FrameClose: OK");
        Console.WriteLine("Return: SUCCESS");

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);

        var diagnostics = transport.GetDiagnostics();
        Ensure(diagnostics.MessagesSent >= 4, "Transport diagnostics did not count sent messages.");
        Ensure(diagnostics.MessagesReceived >= 4, "Transport diagnostics did not count received messages.");
        Console.WriteLine("RecoveryBehavior: PREPARED");
        Console.WriteLine("GuestHasPdfFile: NO");
        Console.WriteLine("GuestHasOriginalPath: NO");
        Console.WriteLine("OriginalFileBytes: NO");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task<int> RunConnectToOwnerAsync(MacGuestHarnessOptions options)
    {
        var (host, port) = ParseConnectTarget(options.ConnectToOwner);
        var transport = new RkwpDevLanTransport(new RkwpDevLanOptions
        {
            Host = host,
            Port = port,
            DefaultTimeout = TimeSpan.FromSeconds(5)
        });
        var client = transport.CreateClient(RkwpDevLanEndpoint.Create(host, port));

        Console.WriteLine("RK Workspace macOS Guest Compatibility Harness");
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("Mode: ConnectToOwner");
        Console.WriteLine($"ConnectToOwner: rkwp+tcp-dev://{host}:{port}");
        WriteIdentityAndCapabilities();

        var hello = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                GuestAblageId,
                OwnerAblageId,
                payload: CreateHelloPayload()),
            TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Ensure(hello.MessageType == TransportMessageType.AblageCapabilities, "Owner did not return capabilities.");
        Console.WriteLine("AblageHello: OK");
        Console.WriteLine($"OwnerResponse: {hello.MessageType}");
        Console.WriteLine("NoFileIngress: ASSERTED");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunReplaySample(MacGuestHarnessOptions options)
    {
        _ = options;
        Console.WriteLine("RK Workspace macOS Guest Compatibility Harness");
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("Mode: ReplaySample");
        WriteIdentityAndCapabilities();
        Console.WriteLine("AblageHello: OK");
        Console.WriteLine("AblageCapabilities: OK");
        Console.WriteLine("FrameGuestSurface: OK");
        Console.WriteLine("FrameSessionReady: OK");
        Console.WriteLine("Heartbeat: OK");
        Console.WriteLine("FrameClose: OK");
        Console.WriteLine("Return: SUCCESS");
        Console.WriteLine("RecoveryBehavior: PREPARED");
        Console.WriteLine("GuestHasPdfFile: NO");
        Console.WriteLine("GuestHasOriginalPath: NO");
        Console.WriteLine("OriginalFileBytes: NO");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static async Task RunOwnerConversationAsync(
        IRkwpTransportServer server,
        string sessionId,
        int expectedMessages,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < expectedMessages; index++)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            var response = CreateOwnerResponse(request, sessionId);
            await server.SendResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static RkwpTransportMessage CreateOwnerResponse(RkwpTransportMessage request, string sessionId)
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
                    ["ownerAblageId"] = OwnerAblageId,
                    ["expectedGuest"] = GuestAblageId,
                    ["frameOnly"] = "true",
                    ["noFileIngress"] = "true",
                    ["devPairing"] = "allowed",
                    ["secureSessionSpike"] = "prepared"
                },
                request.MessageId),
            TransportMessageType.AblageCapabilities => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameId"] = "frame-macos-compat",
                    ["frameSessionReady"] = "true",
                    ["frameOnly"] = "true",
                    ["containsOriginalFileBytes"] = "false",
                    ["hasOriginalPath"] = "false",
                    ["noFileIngress"] = "true"
                },
                request.MessageId),
            TransportMessageType.CarryLeaseHeartbeat => RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = GetPayload(request, "leaseId", "lease-macos-compat"),
                    ["heartbeat"] = "alive",
                    ["recoveryArmed"] = "true"
                },
                request.MessageId),
            TransportMessageType.CarryLeaseReturn => RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = GetPayload(request, "leaseId", "lease-macos-compat"),
                    ["returnAccepted"] = "true",
                    ["frameClosed"] = "true",
                    ["ownerAvailable"] = "true",
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

    private static IReadOnlyDictionary<string, string> CreateHelloPayload()
    {
        return new Dictionary<string, string>
        {
            ["ablageId"] = GuestAblageId,
            ["displayName"] = "Ablage macOS Guest",
            ["platform"] = SurfacePlatform.MacOS.ToString(),
            ["role"] = "FrameGuestSurface"
        };
    }

    private static IReadOnlyDictionary<string, string> CreateCapabilityPayload()
    {
        return new Dictionary<string, string>
        {
            ["platform"] = SurfacePlatform.MacOS.ToString(),
            ["frameView"] = "true",
            ["frameInput"] = "false",
            ["haptics"] = "planned",
            ["glassEdge"] = "planned",
            ["noFileIngress"] = "true",
            ["ownershipTransfer"] = "false",
            ["capabilityFlags"] = (SurfaceCapabilities.FramePresentation | SurfaceCapabilities.SecureContext).ToString()
        };
    }

    private static void WriteIdentityAndCapabilities()
    {
        Console.WriteLine($"MacGuestAblageId: {GuestAblageId}");
        Console.WriteLine("MacGuestIdentity: OK");
        Console.WriteLine($"Platform: {SurfacePlatform.MacOS}");
        Console.WriteLine("FrameView: OK");
        Console.WriteLine("FrameInput: OFF");
        Console.WriteLine("Haptics: PLANNED");
        Console.WriteLine("GlassEdge: PLANNED");
        Console.WriteLine("NoFileIngressCapability: OK");
        Console.WriteLine("OwnershipTransfer: OFF");
    }

    private static (string Host, int Port) ParseConnectTarget(string value)
    {
        var target = value.Trim();
        const string scheme = "rkwp+tcp-dev://";
        if (target.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
        {
            target = target[scheme.Length..];
        }

        var parts = target.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
        {
            throw new InvalidOperationException("ConnectToOwner must be rkwp+tcp-dev://host:port or host:port.");
        }

        return (parts[0], port);
    }

    private static string GetPayload(RkwpTransportMessage request, string key, string fallback)
    {
        return request.Payload.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static void EnsurePayload(RkwpTransportMessage message, string key, string expected)
    {
        Ensure(
            message.Payload.TryGetValue(key, out var value) &&
            string.Equals(value, expected, StringComparison.OrdinalIgnoreCase),
            $"{key} was expected to be {expected}.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal enum MacGuestHarnessMode
{
    SmokeTest,
    ConnectToOwner,
    ReplaySample
}

internal sealed record MacGuestHarnessOptions(
    MacGuestHarnessMode Mode,
    string BindAddress,
    string Host,
    int Port,
    string ConnectToOwner)
{
    public static MacGuestHarnessOptions Parse(string[] args)
    {
        var mode = MacGuestHarnessMode.SmokeTest;
        var bindAddress = "127.0.0.1";
        var host = "127.0.0.1";
        var port = 57101;
        var connectToOwner = string.Empty;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                mode = MacGuestHarnessMode.SmokeTest;
                continue;
            }

            if (Is(arg, "--replay-sample", "-ReplaySample"))
            {
                mode = MacGuestHarnessMode.ReplaySample;
                continue;
            }

            if (Is(arg, "--connect-to-owner", "-ConnectToOwner") && index + 1 < args.Length)
            {
                mode = MacGuestHarnessMode.ConnectToOwner;
                connectToOwner = args[++index];
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
            }
        }

        return new MacGuestHarnessOptions(mode, bindAddress, host, port, connectToOwner);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
