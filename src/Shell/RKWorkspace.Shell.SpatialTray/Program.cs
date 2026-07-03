using RKWorkspace.Shell.SpatialTray;

var options = SpatialTrayCliOptions.Parse(args);
if (options.Help)
{
    PrintHelp();
    return 0;
}

if (options.SmokeTest)
{
    return await SpatialTraySmokeTest.RunAsync(options.Port);
}

var configuration = new SpatialTrayConfiguration
{
    Port = options.Port ?? SpatialTrayConfiguration.DefaultPort
};

await using var server = new SpatialTrayServer(configuration);
using var cancellation = new CancellationTokenSource();
ConsoleCancelEventHandler handler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Console.CancelKeyPress += handler;
try
{
    await server.StartAsync(cancellation.Token);
    var diagnostics = server.GetDiagnostics();

    Console.WriteLine("RK Workspace Spatial Room Session");
    Console.WriteLine("Raumzustand: aktiv");
    Console.WriteLine("Ablage Handy oeffnen:");
    Console.WriteLine(diagnostics.TrayUrl);
    Console.WriteLine("Ablage Monitor oeffnen:");
    Console.WriteLine(diagnostics.AblageUrl);
    Console.WriteLine("Falls das Handy/Tablet die Adresse nicht erreicht, im selben WLAN die lokale Rechner-IP verwenden.");
    Console.WriteLine("Ctrl+C beendet die lokale Raum-Session.");

    while (!cancellation.IsCancellationRequested)
    {
        await Task.Delay(250, cancellation.Token);
    }
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
}
finally
{
    Console.CancelKeyPress -= handler;
    await server.StopAsync(CancellationToken.None);
}

return 0;

static void PrintHelp()
{
    Console.WriteLine("RK Workspace Spatial Room Session");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --smoke-test   Run the local smoke test and stop.");
    Console.WriteLine("  --port <port>  Use a specific local port.");
    Console.WriteLine("  --help         Show help.");
}

internal sealed record SpatialTrayCliOptions
{
    public bool SmokeTest { get; init; }

    public int? Port { get; init; }

    public bool Help { get; init; }

    public static SpatialTrayCliOptions Parse(IReadOnlyList<string> args)
    {
        var options = new SpatialTrayCliOptions();
        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (Is(arg, "--smoke-test"))
            {
                options = options with { SmokeTest = true };
                continue;
            }

            if (Is(arg, "--help"))
            {
                options = options with { Help = true };
                continue;
            }

            if (Is(arg, "--port"))
            {
                var value = ReadValue(args, ref index, arg);
                if (!int.TryParse(value, out var port))
                {
                    throw new SpatialTrayException($"{arg} requires a numeric value.");
                }

                options = options with { Port = port };
                continue;
            }

            throw new SpatialTrayException($"Unsupported option '{arg}'.");
        }

        return options;
    }

    private static string ReadValue(IReadOnlyList<string> args, ref int index, string option)
    {
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new SpatialTrayException($"{option} requires a value.");
        }

        index++;
        return args[index];
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
