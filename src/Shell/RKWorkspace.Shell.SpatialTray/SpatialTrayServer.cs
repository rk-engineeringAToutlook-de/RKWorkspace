using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTrayServer : IAsyncDisposable
{
    private readonly SpatialTrayConfiguration _configuration;
    private readonly SpatialTraySession _session;
    private WebApplication? _app;
    private DateTimeOffset _startedAt;

    public SpatialTrayServer(SpatialTrayConfiguration configuration)
    {
        _configuration = configuration.Validate();
        _session = new SpatialTraySession(_configuration);
    }

    public SpatialTrayDiagnostics GetDiagnostics()
    {
        var ip = GetLocalIpAddress() ?? "localhost";
        return new SpatialTrayDiagnostics
        {
            Port = _configuration.Port,
            TrayUrl = $"http://{ip}:{_configuration.Port}/tray",
            LocalTrayUrl = $"http://localhost:{_configuration.Port}/tray",
            AblageUrl = $"http://localhost:{_configuration.Port}/ablage",
            State = _session.CurrentState,
            ThingName = _configuration.ThingName,
            DesktopAblageName = _configuration.DesktopAblageName,
            StartedAt = _startedAt
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = Array.Empty<string>(),
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://0.0.0.0:{_configuration.Port}");
        builder.Services.AddRouting();

        var app = builder.Build();
        MapEndpoints(app);
        _startedAt = DateTimeOffset.UtcNow;
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
        app.MapGet("/", () => Results.Redirect("/tray"));
        app.MapGet("/health", () => Results.Json(new { status = "OK" }));
        app.MapGet("/api/state", () => Results.Json(_session.Snapshot()));
        app.MapPost("/api/pick", () =>
        {
            _session.Pick();
            return Results.Json(_session.Snapshot());
        });
        app.MapPost("/api/carry", () =>
        {
            _session.Carry();
            return Results.Json(_session.Snapshot());
        });
        app.MapPost("/api/near", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<AblageRequest>();
            _session.NearAblage(payload?.Ablage ?? _configuration.DesktopAblageName);
            return Results.Json(_session.Snapshot());
        });
        app.MapPost("/api/release", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<AblageRequest>();
            _session.Release(payload?.X, payload?.Y, payload?.Ablage, payload?.PlaceOnAblage == true);
            return Results.Json(_session.Snapshot());
        });
        app.MapPost("/api/place", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<AblageRequest>();
            _session.Place(payload?.Ablage);
            return Results.Json(_session.Snapshot());
        });
        app.MapPost("/api/cancel", () =>
        {
            _session.Cancel();
            return Results.Json(_session.Snapshot());
        });
        app.MapGet("/tray", () => ServeWebFile("index.html", "text/html; charset=utf-8"));
        app.MapGet("/tray.css", () => ServeWebFile("tray.css", "text/css; charset=utf-8"));
        app.MapGet("/tray.js", () => ServeWebFile("tray.js", "application/javascript; charset=utf-8"));
        app.MapGet("/ablage", () => Results.Content(RenderAblage(), "text/html; charset=utf-8"));
    }

    private IResult ServeWebFile(string fileName, string contentType)
    {
        var path = Path.Combine(_configuration.WebRootPath, fileName);
        if (!File.Exists(path))
        {
            return Results.NotFound();
        }

        return Results.Bytes(File.ReadAllBytes(path), contentType);
    }

    private string RenderAblage()
    {
        var placed = _session.DesktopAblageHasThing;
        var status = placed
            ? $"Hier liegt jetzt: {_configuration.ThingName}"
            : $"{_configuration.DesktopAblageName} bereit";
        var detail = placed ? "Abgelegt" : "Ruhig warten";

        return $$"""
            <!doctype html>
            <html lang="de">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Ablage</title>
                <style>
                    body {
                        margin: 0;
                        min-height: 100vh;
                        display: grid;
                        place-items: center;
                        background: #101820;
                        color: #eef5f0;
                        font-family: "Segoe UI", Arial, sans-serif;
                    }
                    main {
                        border: 1px solid rgba(190, 220, 205, .32);
                        border-radius: 8px;
                        padding: 28px 32px;
                        background: rgba(30, 45, 54, .82);
                        box-shadow: 0 18px 60px rgba(0, 0, 0, .28);
                    }
                    h1 { margin: 0 0 10px; font-size: 24px; }
                    p { margin: 0; color: #b8cbc2; font-size: 16px; }
                </style>
            </head>
            <body>
                <main>
                    <h1>{{WebUtility.HtmlEncode(status)}}</h1>
                    <p>{{WebUtility.HtmlEncode(detail)}}</p>
                </main>
            </body>
            </html>
            """;
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

    private sealed record AblageRequest(string? Ablage, double? X, double? Y, bool? PlaceOnAblage);
}
