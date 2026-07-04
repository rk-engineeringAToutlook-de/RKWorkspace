using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var options = Real3DLensOptions.Parse(args);
if (options.Help)
{
    PrintHelp();
    return 0;
}

if (options.SmokeTest)
{
    return await Real3DLensSmokeTest.RunAsync(options.Port);
}

var port = options.Port is > 0 ? options.Port.Value : Real3DLensServer.DefaultPort;
await using var server = new Real3DLensServer(port);
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
    var url = server.GetLocalUrl();
    var networkUrl = server.GetNetworkUrl();
    Console.WriteLine("RK Workspace Real3D Lens Renderer");
    Console.WriteLine("----------------------------------");
    Console.WriteLine($"Local: {url}");
    Console.WriteLine($"Network: {networkUrl}");
    Console.WriteLine("Renderer: WebGL / Three.js / physically based glass");
    Console.WriteLine("Ctrl+C beendet den Real3D-Look-Prototyp.");

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
    Console.WriteLine("RK Workspace Real3D Lens Renderer");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --smoke-test   Run the local smoke test and stop.");
    Console.WriteLine("  --port <port>  Use a specific local port.");
    Console.WriteLine("  --help         Show help.");
}

internal sealed class Real3DLensServer(int port) : IAsyncDisposable
{
    public const int DefaultPort = 5137;
    private WebApplication? _app;

    public string GetLocalUrl() => $"http://localhost:{port}/real3d";

    public string GetNetworkUrl() => $"http://{GetLocalIpAddress() ?? "localhost"}:{port}/real3d";

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        var app = builder.Build();
        MapEndpoints(app);
        _app = app;
        await app.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync(cancellationToken);
        await _app.DisposeAsync();
        _app = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private void MapEndpoints(WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect("/real3d"));
        app.MapGet("/real3d", () => ServeWebFile("index.html", "text/html; charset=utf-8"));
        app.MapGet("/real3d.css", () => ServeWebFile("real3d.css", "text/css; charset=utf-8"));
        app.MapGet("/real3d.js", () => ServeWebFile("real3d.js", "application/javascript; charset=utf-8"));
        app.MapGet("/favicon.ico", () => Results.NoContent());
        app.MapGet("/health", () => Results.Json(new
        {
            status = "OK",
            renderer = "real-3d-lens",
            webgl = true,
            physicalGlass = true,
            desktopCapture = "optional"
        }));
    }

    private static IResult ServeWebFile(string fileName, string contentType)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Web", fileName);
        if (!File.Exists(path))
        {
            return Results.NotFound();
        }

        return Results.Bytes(File.ReadAllBytes(path), contentType);
    }

    private static string? GetLocalIpAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
            .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.Address.ToString())
            .FirstOrDefault(address => !IPAddress.IsLoopback(IPAddress.Parse(address)));
    }
}

internal sealed record Real3DLensOptions
{
    public bool SmokeTest { get; init; }

    public int? Port { get; init; }

    public bool Help { get; init; }

    public static Real3DLensOptions Parse(IReadOnlyList<string> args)
    {
        var options = new Real3DLensOptions();
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
                if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{arg} requires a value.");
                }

                index++;
                if (!int.TryParse(args[index], out var port))
                {
                    throw new InvalidOperationException($"{arg} requires a numeric value.");
                }

                options = options with { Port = port };
                continue;
            }

            throw new InvalidOperationException($"Unsupported option '{arg}'.");
        }

        return options;
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
