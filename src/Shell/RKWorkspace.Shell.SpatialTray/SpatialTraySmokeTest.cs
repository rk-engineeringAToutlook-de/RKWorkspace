using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RKWorkspace.Shell.SpatialTray;

public static class SpatialTraySmokeTest
{
    public static async Task<int> RunAsync(int? requestedPort = null)
    {
        var port = requestedPort is > 0 ? requestedPort.Value : FindAvailablePort();
        var configuration = new SpatialTrayConfiguration
        {
            Port = port
        };

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await using var server = new SpatialTrayServer(configuration);
        try
        {
            await server.StartAsync(timeout.Token);
            using var client = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}"),
                Timeout = TimeSpan.FromSeconds(5)
            };

            var healthOk = await GetContainsAsync(client, "/health", "OK", timeout.Token);
            var handySurfaceOk = await GetContainsAsync(client, "/surface/handy", "Ablage im Raum", timeout.Token);
            var monitorSurfaceOk = await GetContainsAsync(client, "/surface/monitor", "Ablage im Raum", timeout.Token);
            var languageOk = await VisibleLanguageIsHumanAsync(client, timeout.Token);

            var initialHandy = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            var roomHasAblagenOk = HasAblage(initialHandy.RootElement, "handy") &&
                HasAblage(initialHandy.RootElement, "monitor");
            var initialThingOk = Thing(initialHandy.RootElement).GetProperty("currentAblageId").GetString() == "handy" &&
                Thing(initialHandy.RootElement).GetProperty("currentState").GetString() == "RestingOnAblage";
            var bubblesOnHandyOk = initialHandy.RootElement.GetProperty("bubbles").GetArrayLength() >= 2;

            using var pickHandy = await PostJsonAsync(client, "/api/pick", new { carrierAblageId = "handy" }, timeout.Token);
            var pickRemovesFromHandyOk = Thing(pickHandy.RootElement).GetProperty("currentAblageId").ValueKind == JsonValueKind.Null &&
                pickHandy.RootElement.GetProperty("activeCarry").GetProperty("state").GetString() == "Picked" &&
                pickHandy.RootElement.GetProperty("activeCarry").GetProperty("sourceAblageId").GetString() == "handy";
            using var handyAfterPick = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            var sourceTraceOk = !handyAfterPick.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean() &&
                handyAfterPick.RootElement.GetProperty("surfaceThing").GetProperty("sourceWasHere").GetBoolean();

            using var approachMonitor = await PostJsonAsync(
                client,
                "/api/approach",
                new { carrierAblageId = "handy", targetAblageId = "monitor" },
                timeout.Token);
            using var monitorPreview = await GetJsonAsync(client, "/api/state?ablage=monitor", timeout.Token);
            var monitorPreviewOk = Thing(approachMonitor.RootElement).GetProperty("previewAblageId").GetString() == "monitor" &&
                Thing(approachMonitor.RootElement).GetProperty("currentState").GetString() == "PreviewOnAblage" &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("isPreviewHere").GetBoolean();

            using var placeMonitor = await PostJsonAsync(
                client,
                "/api/place",
                new { carrierAblageId = "handy", targetAblageId = "monitor", x = 0.5, y = 0.5 },
                timeout.Token);
            using var handyAfterMonitorPlace = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            using var monitorAfterPlace = await GetJsonAsync(client, "/api/state?ablage=monitor", timeout.Token);
            var placeMonitorOk = Thing(placeMonitor.RootElement).GetProperty("currentAblageId").GetString() == "monitor" &&
                Thing(placeMonitor.RootElement).GetProperty("currentState").GetString() == "PlacedOnAblage" &&
                monitorAfterPlace.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean() &&
                !handyAfterMonitorPlace.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean();

            using var pickMonitor = await PostJsonAsync(client, "/api/pick", new { carrierAblageId = "monitor" }, timeout.Token);
            var pickFromMonitorOk = Thing(pickMonitor.RootElement).GetProperty("currentAblageId").ValueKind == JsonValueKind.Null &&
                pickMonitor.RootElement.GetProperty("activeCarry").GetProperty("sourceAblageId").GetString() == "monitor" &&
                pickMonitor.RootElement.GetProperty("activeCarry").GetProperty("carrierAblageId").GetString() == "monitor";

            using var approachHandy = await PostJsonAsync(
                client,
                "/api/approach",
                new { carrierAblageId = "monitor", targetAblageId = "handy" },
                timeout.Token);
            using var handyPreview = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            var handyPreviewOk = Thing(approachHandy.RootElement).GetProperty("previewAblageId").GetString() == "handy" &&
                handyPreview.RootElement.GetProperty("surfaceThing").GetProperty("isPreviewHere").GetBoolean();

            using var placeHandy = await PostJsonAsync(
                client,
                "/api/place",
                new { carrierAblageId = "monitor", targetAblageId = "handy", x = 0.5, y = 0.5 },
                timeout.Token);
            var placeHandyOk = Thing(placeHandy.RootElement).GetProperty("currentAblageId").GetString() == "handy" &&
                Thing(placeHandy.RootElement).GetProperty("currentState").GetString() == "PlacedOnAblage";

            using (await PostJsonAsync(client, "/api/pick", new { carrierAblageId = "handy" }, timeout.Token))
            {
            }

            using var freePlace = await PostJsonAsync(
                client,
                "/api/release",
                new { carrierAblageId = "handy", x = 0.25, y = 0.66, placeOnAblage = false },
                timeout.Token);
            var freePlaceOk = Thing(freePlace.RootElement).GetProperty("currentState").GetString() == "FreePlaced" &&
                Thing(freePlace.RootElement).GetProperty("currentAblageId").GetString() == "handy" &&
                Thing(freePlace.RootElement).GetProperty("positionOnAblage").GetProperty("x").GetDouble() == 0.25;

            using (await PostJsonAsync(client, "/api/pick", new { carrierAblageId = "handy" }, timeout.Token))
            {
            }

            using var cancel = await PostJsonAsync(client, "/api/cancel", new { carrierAblageId = "handy" }, timeout.Token);
            var cancelOk = Thing(cancel.RootElement).GetProperty("currentAblageId").GetString() == "handy" &&
                Thing(cancel.RootElement).GetProperty("currentState").GetString() == "Cancelled" &&
                cancel.RootElement.GetProperty("activeCarry").GetProperty("state").GetString() == "Cancelled";

            var success = healthOk &&
                handySurfaceOk &&
                monitorSurfaceOk &&
                roomHasAblagenOk &&
                initialThingOk &&
                pickRemovesFromHandyOk &&
                sourceTraceOk &&
                monitorPreviewOk &&
                placeMonitorOk &&
                pickFromMonitorOk &&
                handyPreviewOk &&
                placeHandyOk &&
                freePlaceOk &&
                cancelOk &&
                languageOk &&
                bubblesOnHandyOk;

            Console.WriteLine("Spatial Room Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Health: {(healthOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Handy surface: {(handySurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Monitor surface: {(monitorSurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Room ablagen: {(roomHasAblagenOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Initial thing: {(initialThingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Pick removes source: {(pickRemovesFromHandyOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Source trace: {(sourceTraceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Monitor preview: {(monitorPreviewOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Place on monitor: {(placeMonitorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Pick from monitor: {(pickFromMonitorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Handy preview: {(handyPreviewOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Place on handy: {(placeHandyOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Free place: {(freePlaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Cancel return: {(cancelOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Human words: {(languageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Surface bubbles: {(bubblesOnHandyOk ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Spatial Room Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Health: FAILED ({ex.Message})");
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }
        finally
        {
            await server.StopAsync(CancellationToken.None);
        }
    }

    private static async Task<bool> GetContainsAsync(
        HttpClient client,
        string path,
        string expected,
        CancellationToken cancellationToken)
    {
        var text = await client.GetStringAsync(path, cancellationToken);
        return text.Contains(expected, StringComparison.Ordinal);
    }

    private static async Task<bool> VisibleLanguageIsHumanAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var text = await client.GetStringAsync("/surface/handy", cancellationToken);
        var forbidden = new[]
        {
            "Transfer",
            "Upload",
            "Download",
            "Sync",
            "Server",
            "Client",
            "Endpoint",
            "Device",
            "Geraet",
            "Gerät",
            "Agent",
            "Workspace",
            "IPC"
        };
        return forbidden.All(word => !text.Contains(word, StringComparison.Ordinal));
    }

    private static async Task<JsonDocument> GetJsonAsync(
        HttpClient client,
        string path,
        CancellationToken cancellationToken)
    {
        var text = await client.GetStringAsync(path, cancellationToken);
        return JsonDocument.Parse(text);
    }

    private static async Task<JsonDocument> PostJsonAsync(
        HttpClient client,
        string path,
        object payload,
        CancellationToken cancellationToken)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
        using var response = await client.PostAsync(path, content, cancellationToken);
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(text);
    }

    private static JsonElement Thing(JsonElement state)
    {
        return state.GetProperty("things").EnumerateArray().Single();
    }

    private static bool HasAblage(JsonElement state, string ablageId)
    {
        return state.GetProperty("ablagen")
            .EnumerateArray()
            .Any(ablage => ablage.GetProperty("ablageId").GetString() == ablageId);
    }

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
