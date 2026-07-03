using RKWorkspace.Shell;

namespace RKWorkspace.Shell.Host;

internal static class Program
{
    private static async Task<int> Main(string[] args)
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
        if (once && status)
        {
            Console.WriteLine("RK Workspace Shell failed: --once and --status cannot be used together.");
            return 1;
        }

        var runtime = new WorkspaceShellRuntime();
        if (once)
        {
            return RunOnce(runtime);
        }

        if (status)
        {
            return RunStatus(runtime);
        }

        return await RunUntilCancelled(runtime);
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
        Console.WriteLine("  --help      Show help.");
    }

    private static bool IsUnknownOption(string value)
    {
        return value.StartsWith("--", StringComparison.Ordinal) &&
            !Is(value, "--once") &&
            !Is(value, "--status") &&
            !Is(value, "--help");
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
