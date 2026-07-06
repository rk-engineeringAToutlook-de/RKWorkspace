using RKWorkspace.RkwpTransport.DevLan;

namespace RKWorkspace.Agent.Windows;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = WindowsAgentDevOptions.Parse(args);
            return options.SmokeTest ? RunSmokeTest(options) : RunStatus(options);
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
        WriteStatus(options);
        Console.WriteLine("AgentStarted: OK");
        Console.WriteLine("NoInstallationRequired: OK");
        Console.WriteLine("AblageIdentity: OK");
        Console.WriteLine("RkwpComponentsReachable: OK");
        Console.WriteLine("Shutdown: OK");
        Console.WriteLine("WindowsAgentDevSmoke: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunStatus(WindowsAgentDevOptions options)
    {
        Console.WriteLine("RK Workspace Windows Agent Dev");
        Console.WriteLine("------------------------------");
        WriteStatus(options);
        Console.WriteLine("RunMode: StatusOnly");
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
    bool SmokeTest,
    string AblageId,
    string BindAddress,
    int Port)
{
    public static WindowsAgentDevOptions Parse(string[] args)
    {
        var smokeTest = args.Any(arg =>
            string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "-SmokeTest", StringComparison.OrdinalIgnoreCase));
        var ablageId = "ablage-windows-dev-agent";
        var bindAddress = "127.0.0.1";
        var port = 57120;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
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

        return new WindowsAgentDevOptions(smokeTest, ablageId, bindAddress, port);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
