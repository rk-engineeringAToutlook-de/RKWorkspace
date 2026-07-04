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
            TrayUrl = $"http://{ip}:{_configuration.Port}/surface/{SpatialTraySession.DefaultAblageId}",
            LocalTrayUrl = $"http://localhost:{_configuration.Port}/surface/{SpatialTraySession.DefaultAblageId}",
            AblageUrl = $"http://localhost:{_configuration.Port}/surface/monitor",
            MobileUrl = $"http://{ip}:{_configuration.Port}/mobile",
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
        app.MapGet("/", () => Results.Redirect($"/surface/{SpatialTraySession.DefaultAblageId}"));
        app.MapGet("/health", () => Results.Json(new { status = "OK" }));
        app.MapGet("/api/state", (HttpRequest request) =>
        {
            var ablageId = request.Query["ablage"].FirstOrDefault();
            return Results.Json(_session.Snapshot(ablageId));
        });
        app.MapPost("/api/pick", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Pick(carrier);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/move", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Move(carrier, payload.X, payload.Y);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/carry", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Move(carrier, payload.X, payload.Y);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/approach", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            var target = payload.TargetAblageId ?? payload.Ablage ?? SpatialTraySession.DefaultTargetAblageId;
            _session.Approach(carrier, target);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/near", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            var target = payload.TargetAblageId ?? payload.Ablage ?? SpatialTraySession.DefaultTargetAblageId;
            _session.Approach(carrier, target);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/release", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Release(payload.X, payload.Y, payload.TargetAblageId ?? payload.Ablage, payload.PlaceOnAblage == true);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/place", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var carrier = payload.CarrierAblageId ?? payload.AblageId ?? payload.TargetAblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Place(payload.TargetAblageId ?? payload.Ablage, payload.X, payload.Y);
            return Results.Json(_session.Snapshot(carrier));
        });
        app.MapPost("/api/cancel", async (HttpRequest request) =>
        {
            var payload = await request.ReadFromJsonAsync<SpatialRoomRequest>() ?? new SpatialRoomRequest();
            var viewer = payload.CarrierAblageId ?? payload.AblageId ?? SpatialTraySession.DefaultAblageId;
            _session.Cancel();
            return Results.Json(_session.Snapshot(viewer));
        });
        app.MapGet("/tray", () => Results.Redirect($"/surface/{SpatialTraySession.DefaultAblageId}"));
        app.MapGet("/ablage", () => Results.Redirect("/surface/monitor"));
        app.MapGet("/mobile", () => ServeWebFile("index.html", "text/html; charset=utf-8"));
        app.MapPost("/api/mobile/gesture", () => Results.Json(MobileSpatialSurfaceModel.ActivateGesture()));
        app.MapGet("/surface/{ablageId}", () => ServeWebFile("index.html", "text/html; charset=utf-8"));
        app.MapGet("/manifest.webmanifest", () => ServeWebFile("manifest.webmanifest", "application/manifest+json; charset=utf-8"));
        app.MapGet("/tray.css", () => ServeWebFile("tray.css", "text/css; charset=utf-8"));
        app.MapGet("/tray.js", () => ServeWebFile("tray.js", "application/javascript; charset=utf-8"));
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

    private static string? GetLocalIpAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
            .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.Address.ToString())
            .FirstOrDefault(address => !IPAddress.IsLoopback(IPAddress.Parse(address)));
    }

    private sealed record SpatialRoomRequest
    {
        public string? AblageId { get; init; }

        public string? CarrierAblageId { get; init; }

        public string? TargetAblageId { get; init; }

        public string? Ablage { get; init; }

        public double? X { get; init; }

        public double? Y { get; init; }

        public bool? PlaceOnAblage { get; init; }
    }
}
