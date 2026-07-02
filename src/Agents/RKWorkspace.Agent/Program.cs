namespace RKWorkspace.Agent;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Any(argument => Is(argument, "--help")))
        {
            PrintHelp();
            return 0;
        }

        var statusMode = args.Any(argument => Is(argument, "--status"));
        var configuration = BuildConfiguration(args) with
        {
            EnableConsoleStatus = !statusMode
        };
        var agent = new AgentRuntime(configuration);

        if (statusMode)
        {
            try
            {
                agent.Start();
                agent.PrintStatus();
                agent.Stop();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        if (args.Any(argument => Is(argument, "--once")))
        {
            return agent.RunOnce();
        }

        return agent.RunUntilCancelled();
    }

    private static AgentConfiguration BuildConfiguration(IEnumerable<string> args)
    {
        var enableDemo = true;
        foreach (var arg in args)
        {
            if (Is(arg, "--demo"))
            {
                enableDemo = true;
            }

            if (Is(arg, "--no-demo"))
            {
                enableDemo = false;
            }
        }

        return new AgentConfiguration
        {
            EnableDemoWorkspace = enableDemo
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("RK Workspace Agent");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --once      Start, print status, stop cleanly.");
        Console.WriteLine("  --status    Print diagnostics and stop.");
        Console.WriteLine("  --demo      Enable local demo workspace registration.");
        Console.WriteLine("  --no-demo   Disable local demo workspace registration.");
        Console.WriteLine("  --help      Show help.");
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
