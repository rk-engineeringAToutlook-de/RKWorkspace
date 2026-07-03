using RKWorkspace.Shell;
using RKWorkspace.Shell.NativeOverlay.Windows;
using RKWorkspace.Shell.Overlay.Windows;

namespace RKWorkspace.Shell.Host;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => Is(argument, "--help")))
        {
            PrintHelp();
            return 0;
        }

        if (args.Any(IsUnknownOption))
        {
            Console.WriteLine("RK Workspace Shell failed: unsupported option.");
            PrintHelp();
            return 1;
        }

        var once = args.Any(argument => Is(argument, "--once"));
        var status = args.Any(argument => Is(argument, "--status"));
        var overlayDemo = args.Any(argument => Is(argument, "--overlay-demo"));
        var overlaySmokeTest = args.Any(argument => Is(argument, "--overlay-smoke-test"));
        var nativeOverlayDemo = args.Any(argument => Is(argument, "--native-overlay-demo"));
        var nativeOverlaySmokeTest = args.Any(argument => Is(argument, "--native-overlay-smoke-test"));
        if (new[] { once, status, overlayDemo, overlaySmokeTest, nativeOverlayDemo, nativeOverlaySmokeTest }.Count(enabled => enabled) > 1)
        {
            Console.WriteLine("RK Workspace Shell failed: shell modes cannot be combined.");
            return 1;
        }

        var runtime = new WorkspaceShellRuntime();
        if (overlayDemo)
        {
            return WorkspaceOverlayApplication.RunDemo();
        }

        if (overlaySmokeTest)
        {
            return WorkspaceOverlayApplication.RunSmokeTest();
        }

        if (nativeOverlayDemo)
        {
            return NativeSpatialOverlayApplication.RunDemo();
        }

        if (nativeOverlaySmokeTest)
        {
            return NativeSpatialOverlayApplication.RunSmokeTest();
        }

        if (once)
        {
            return RunOnce(runtime);
        }

        if (status)
        {
            return RunStatus(runtime);
        }

        return RunUntilCancelled(runtime).GetAwaiter().GetResult();
    }

    private static int RunOnce(IWorkspaceShellRuntime runtime)
    {
        try
        {
            var exitCode = runtime.RunOnce();
            PrintStatus(runtime.GetDiagnostics(), includeResult: exitCode == 0);
            runtime.Stop();
            return exitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Shell failed: {ex.Message}");
            return 1;
        }
    }

    private static int RunStatus(IWorkspaceShellRuntime runtime)
    {
        try
        {
            runtime.Start();
            PrintStatus(runtime.GetDiagnostics(), includeResult: false);
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Shell failed: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunUntilCancelled(IWorkspaceShellRuntime runtime)
    {
        try
        {
            runtime.Start();
            PrintStatus(runtime.GetDiagnostics(), includeResult: false);
            Console.WriteLine("Ctrl+C beendet die Workspace Shell sauber.");
            return await runtime.RunUntilCancelled();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Shell failed: {ex.Message}");
            return 1;
        }
    }

    private static void PrintStatus(WorkspaceShellDiagnostics diagnostics, bool includeResult)
    {
        Console.WriteLine("RK Workspace Shell");
        Console.WriteLine($"State: {diagnostics.State}");
        Console.WriteLine($"Session: {diagnostics.Session}");
        Console.WriteLine($"CarryState: {diagnostics.CarryState}");
        Console.WriteLine($"Overlay: {diagnostics.Overlay}");
        Console.WriteLine($"ProductMode: {diagnostics.ProductMode}");

        if (includeResult)
        {
            Console.WriteLine("RESULT: SUCCESS");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("RK Workspace Shell");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --once      Start the shell, print status, then stop cleanly.");
        Console.WriteLine("  --status    Print prepared shell diagnostics and stop.");
        Console.WriteLine("  --overlay-demo");
        Console.WriteLine("              Start the transparent Workspace Overlay prototype.");
        Console.WriteLine("  --overlay-smoke-test");
        Console.WriteLine("              Initialize the Workspace Overlay prototype and stop.");
        Console.WriteLine("  --native-overlay-demo");
        Console.WriteLine("              Start the native transparent Spatial Overlay slice.");
        Console.WriteLine("  --native-overlay-smoke-test");
        Console.WriteLine("              Initialize the native Spatial Overlay slice and stop.");
        Console.WriteLine("  --help      Show help.");
    }

    private static bool IsUnknownOption(string value)
    {
        return value.StartsWith("--", StringComparison.Ordinal) &&
            !Is(value, "--once") &&
            !Is(value, "--status") &&
            !Is(value, "--overlay-demo") &&
            !Is(value, "--overlay-smoke-test") &&
            !Is(value, "--native-overlay-demo") &&
            !Is(value, "--native-overlay-smoke-test") &&
            !Is(value, "--help");
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
