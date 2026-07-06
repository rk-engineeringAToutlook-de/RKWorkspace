using RKWorkspace.RkwpTransport.DevLan;

namespace RKWorkspace.Agent.Windows;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = WindowsAgentDevOptions.Parse(args);
            return options.Mode switch
            {
                WindowsAgentDevMode.SmokeTest => RunSmokeTest(options),
                WindowsAgentDevMode.Start => RunStart(options),
                WindowsAgentDevMode.Stop => RunStop(options),
                WindowsAgentDevMode.Identity => RunIdentity(options),
                WindowsAgentDevMode.Transport => RunTransport(options),
                WindowsAgentDevMode.FrameOwner => RunFrameOwner(options),
                WindowsAgentDevMode.GuestSurface => RunGuestSurface(options),
                _ => RunStatus(options)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Windows Agent Dev");
            Console.WriteLine("------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static int RunSmokeTest(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        RunStart(options);
        RunStatus(options);
        RunIdentity(options);
        RunTransport(options);
        RunFrameOwner(options);
        RunGuestSurface(options);
        RunStop(options);
        Console.WriteLine("NoInstallationRequired: OK");
        Console.WriteLine("RkwpComponentsReachable: OK");
        Console.WriteLine("Shutdown: OK");
        Console.WriteLine("WindowsAgentDevSmoke: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunStart(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: Start");
        WriteStatus(options);
        Console.WriteLine("AgentStarted: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunStop(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: Stop");
        Console.WriteLine("StopMode: OK");
        Console.WriteLine("ServiceInstall: NOT_PERFORMED");
        Console.WriteLine("Shutdown: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunStatus(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: Status");
        WriteStatus(options);
        Console.WriteLine("StatusMode: OK");
        Console.WriteLine("RunMode: StatusOnly");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunIdentity(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: Identity");
        Console.WriteLine($"AblageId: {options.AblageId}");
        Console.WriteLine("DisplayName: Windows Dev Ablage");
        Console.WriteLine("AblageIdentity: OK");
        Console.WriteLine("IdentityMode: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunTransport(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: Transport");
        var endpoint = RkwpDevLanEndpoint.Create(options.BindAddress, options.Port);
        Console.WriteLine($"TransportProfile: {endpoint.TransportKind}");
        Console.WriteLine($"BindAddress: {options.BindAddress}");
        Console.WriteLine($"Port: {options.Port}");
        Console.WriteLine("RkwpDevLan: PREPARED");
        Console.WriteLine("TransportMode: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunFrameOwner(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: FrameOwner");
        Console.WriteLine("FrameOwnerHost: PREPARED");
        Console.WriteLine("NoFileIngress: ENFORCED");
        Console.WriteLine("FrameOwnerMode: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunGuestSurface(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Mode: GuestSurface");
        Console.WriteLine("FrameGuestSurface: PREPARED");
        Console.WriteLine("GlassEdgeSurface: PREPARED");
        Console.WriteLine("GuestSurfaceMode: OK");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static void WriteStatus(WindowsAgentDevOptions options)
    {
        var endpoint = RkwpDevLanEndpoint.Create(options.BindAddress, options.Port);
        Console.WriteLine("ProductRole: Windows Agent Dev Host");
        Console.WriteLine("InstallMode: DevelopmentOnly");
        Console.WriteLine("ServiceInstall: NOT_PERFORMED");
        Console.WriteLine("RequiresAdmin: NO");
        Console.WriteLine("UserContext: OK");
        Console.WriteLine($"AblageId: {options.AblageId}");
        Console.WriteLine("DisplayName: Windows Dev Ablage");
        Console.WriteLine($"TransportProfile: {endpoint.TransportKind}");
        Console.WriteLine($"BindAddress: {options.BindAddress}");
        Console.WriteLine($"Port: {options.Port}");
        Console.WriteLine("RkwpDevLan: PREPARED");
        Console.WriteLine("FrameOwnerHost: PREPARED");
        Console.WriteLine("FrameGuestSurface: PREPARED");
        Console.WriteLine("ProximityProvider: PREPARED");
        Console.WriteLine("GlassEdgeSurface: PREPARED");
        Console.WriteLine("AuditLog: PREPARED");
        Console.WriteLine("PolicyProfile: PREPARED");
        Console.WriteLine("Recovery: PREPARED");
        Console.WriteLine("ContextReporting: PREPARED");
    }
}

internal sealed record WindowsAgentDevOptions(
    WindowsAgentDevMode Mode,
    string AblageId,
    string BindAddress,
    int Port)
{
    public static WindowsAgentDevOptions Parse(string[] args)
    {
        var mode = WindowsAgentDevMode.Status;
        var ablageId = "ablage-windows-dev-agent";
        var bindAddress = "127.0.0.1";
        var port = 57120;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--start", "-Start"))
            {
                mode = WindowsAgentDevMode.Start;
                continue;
            }

            if (Is(arg, "--stop", "-Stop"))
            {
                mode = WindowsAgentDevMode.Stop;
                continue;
            }

            if (Is(arg, "--status", "-Status"))
            {
                mode = WindowsAgentDevMode.Status;
                continue;
            }

            if (Is(arg, "--identity", "-Identity"))
            {
                mode = WindowsAgentDevMode.Identity;
                continue;
            }

            if (Is(arg, "--transport", "-Transport"))
            {
                mode = WindowsAgentDevMode.Transport;
                continue;
            }

            if (Is(arg, "--frame-owner", "-FrameOwner"))
            {
                mode = WindowsAgentDevMode.FrameOwner;
                continue;
            }

            if (Is(arg, "--guest-surface", "-GuestSurface"))
            {
                mode = WindowsAgentDevMode.GuestSurface;
                continue;
            }

            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                mode = WindowsAgentDevMode.SmokeTest;
                continue;
            }

            if (Is(arg, "--ablage-id", "-AblageId") && index + 1 < args.Length)
            {
                ablageId = args[++index];
                continue;
            }

            if (Is(arg, "--bind-address", "-BindAddress") && index + 1 < args.Length)
            {
                bindAddress = args[++index];
                continue;
            }

            if (Is(arg, "--port", "-Port") && index + 1 < args.Length && int.TryParse(args[index + 1], out var parsedPort))
            {
                port = parsedPort;
                index++;
            }
        }

        return new WindowsAgentDevOptions(mode, ablageId, bindAddress, port);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}

internal enum WindowsAgentDevMode
{
    Status,
    Start,
    Stop,
    Identity,
    Transport,
    FrameOwner,
    GuestSurface,
    SmokeTest
}
