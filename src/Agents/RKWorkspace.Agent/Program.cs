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

        var options = AgentCliOptions.Parse(args);
        var statusMode = args.Any(argument => Is(argument, "--status"));
        var ipcMode = !string.IsNullOrWhiteSpace(options.IpcServer) ||
            !string.IsNullOrWhiteSpace(options.IpcClient);
        var configuration = BuildConfiguration(options, args) with
        {
            EnableConsoleStatus = !statusMode && !ipcMode
        };
        var agent = new AgentRuntime(configuration);

        if (!string.IsNullOrWhiteSpace(options.IpcServer) &&
            !string.IsNullOrWhiteSpace(options.IpcClient))
        {
            Console.WriteLine("RK Workspace Agent failed: --ipc-server and --ipc-client cannot be used together.");
            return 1;
        }

        if (!string.IsNullOrWhiteSpace(options.IpcServer))
        {
            return new AgentIpcSession(agent)
                .RunServerAsync(options.IpcServer, options.IpcStopAfterTransfer)
                .GetAwaiter()
                .GetResult();
        }

        if (!string.IsNullOrWhiteSpace(options.IpcClient))
        {
            return new AgentIpcSession(agent)
                .RunClientAsync(options.IpcClient, options.TargetAgentId)
                .GetAwaiter()
                .GetResult();
        }

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

    private static AgentConfiguration BuildConfiguration(
        AgentCliOptions options,
        IEnumerable<string> args)
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

        var configuration = new AgentConfiguration
        {
            EnableDemoWorkspace = enableDemo,
            AgentId = string.IsNullOrWhiteSpace(options.AgentId)
                ? "rkws-agent-local"
                : options.AgentId,
            WorkspaceName = string.IsNullOrWhiteSpace(options.WorkspaceName)
                ? "RKWS Local Workspace"
                : options.WorkspaceName,
            WorkspacePosition = options.Position ?? RKWorkspace.Core.Workspaces.WorkspacePosition.Center
        };

        return configuration with
        {
            DisplayName = string.IsNullOrWhiteSpace(options.AgentId)
                ? configuration.DisplayName
                : $"RKWS Agent {options.AgentId}"
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
        Console.WriteLine("  --agent-id <id>");
        Console.WriteLine("  --workspace-name <name>");
        Console.WriteLine("  --position <Left|Right|Center>");
        Console.WriteLine("  --ipc-server <pipeName>");
        Console.WriteLine("  --ipc-client <pipeName>");
        Console.WriteLine("  --target-agent-id <id>");
        Console.WriteLine("  --ipc-stop-after-transfer");
        Console.WriteLine("  --help      Show help.");
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record AgentCliOptions
    {
        public string AgentId { get; init; } = string.Empty;

        public string WorkspaceName { get; init; } = string.Empty;

        public RKWorkspace.Core.Workspaces.WorkspacePosition? Position { get; init; }

        public string IpcServer { get; init; } = string.Empty;

        public string IpcClient { get; init; } = string.Empty;

        public string TargetAgentId { get; init; } = string.Empty;

        public bool IpcStopAfterTransfer { get; init; }

        public static AgentCliOptions Parse(IReadOnlyList<string> args)
        {
            var options = new AgentCliOptions();
            for (var index = 0; index < args.Count; index++)
            {
                var arg = args[index];
                if (Is(arg, "--agent-id"))
                {
                    options = options with { AgentId = ReadValue(args, ref index, arg) };
                    continue;
                }

                if (Is(arg, "--workspace-name"))
                {
                    options = options with { WorkspaceName = ReadValue(args, ref index, arg) };
                    continue;
                }

                if (Is(arg, "--position"))
                {
                    var value = ReadValue(args, ref index, arg);
                    if (!Enum.TryParse<RKWorkspace.Core.Workspaces.WorkspacePosition>(
                        value,
                        ignoreCase: true,
                        out var position))
                    {
                        throw new AgentException($"Unsupported workspace position '{value}'.");
                    }

                    options = options with { Position = position };
                    continue;
                }

                if (Is(arg, "--ipc-server"))
                {
                    options = options with { IpcServer = ReadValue(args, ref index, arg) };
                    continue;
                }

                if (Is(arg, "--ipc-client"))
                {
                    options = options with { IpcClient = ReadValue(args, ref index, arg) };
                    continue;
                }

                if (Is(arg, "--target-agent-id"))
                {
                    options = options with { TargetAgentId = ReadValue(args, ref index, arg) };
                    continue;
                }

                if (Is(arg, "--ipc-stop-after-transfer"))
                {
                    options = options with { IpcStopAfterTransfer = true };
                }
            }

            return options;
        }

        private static string ReadValue(IReadOnlyList<string> args, ref int index, string option)
        {
            if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new AgentException($"{option} requires a value.");
            }

            index++;
            return args[index];
        }
    }
}
