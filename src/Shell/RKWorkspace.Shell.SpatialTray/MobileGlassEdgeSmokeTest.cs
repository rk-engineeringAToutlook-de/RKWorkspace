using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RKWorkspace.Shell.SpatialTray;

public static class MobileGlassEdgeSmokeTest
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

            var mobileSurfaceOk = await GetContainsAsync(client, "/mobile", "Ablage im Raum", timeout.Token);
            var staticPreparedOk = await StaticMobileGlassEdgePreparedAsync(client, timeout.Token);
            using var gesture = await PostJsonAsync(client, "/api/mobile/glass-edge-gesture", new { gesture = "long-touch" }, timeout.Token);
            var root = gesture.RootElement;
            var modeOk = root.GetProperty("mobileGlassEdgeMode").GetString() == "Active";
            var exactlyOneEdgeOk = root.GetProperty("glassEdgeVisible").GetBoolean() &&
                root.GetProperty("visibleGlassEdges").GetInt32() == 1;
            var nearest = root.GetProperty("nearestAblage");
            var nearestOk = nearest.GetProperty("ablageId").GetString() == "ablage-macos" &&
                nearest.GetProperty("direction").GetString() == "Right" &&
                nearest.GetProperty("platform").GetString() == "MacOS" &&
                nearest.GetProperty("source").GetString() == "Simulated";
            var edge = root.GetProperty("glassEdge");
            var edgeOk = edge.GetProperty("direction").GetString() == "Right" &&
                edge.GetProperty("counterDirection").GetString() == "Left" &&
                edge.GetProperty("opacity").GetDouble() > 0 &&
                edge.GetProperty("thickness").GetDouble() > 0 &&
                edge.GetProperty("absorptionVariants").GetArrayLength() == 3 &&
                edge.GetProperty("textWhenNear").GetString() == "Hier ablegen";
            var ghost = root.GetProperty("incomingGhost");
            var ghostOk = ghost.GetProperty("visible").GetBoolean() &&
                ghost.GetProperty("counterEdge").GetString() == "Left" &&
                ghost.GetProperty("placement").GetProperty("simulated").GetBoolean();
            var haptics = root.GetProperty("haptics");
            var hapticsOk = haptics.GetProperty("requested").GetBoolean() &&
                haptics.GetProperty("skippedSafely").GetBoolean() &&
                haptics.GetProperty("moments").GetArrayLength() >= 6;
            var noTechnicalWordsOk = !root.GetProperty("forbiddenWordsVisible").GetBoolean() &&
                await VisibleLanguageIsHumanAsync(client, timeout.Token);

            var success = mobileSurfaceOk &&
                staticPreparedOk &&
                modeOk &&
                exactlyOneEdgeOk &&
                nearestOk &&
                edgeOk &&
                ghostOk &&
                hapticsOk &&
                noTechnicalWordsOk;

            Console.WriteLine("RK Workspace Mobile Glass Edge Smoke Test");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine($"MobileSurface: {(mobileSurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"StaticGlassEdgeUi: {(staticPreparedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"MobileGlassEdgeMode: {(modeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"SingleGlassEdge: {(exactlyOneEdgeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NearestAblage: {(nearestOk ? "OK" : "FAILED")}");
            Console.WriteLine($"GlassEdge: {(edgeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"CounterEdgeGhost: {(ghostOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Haptics: {(hapticsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NoTechnicalWords: {(noTechnicalWordsOk ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "MobileGlassEdgeSmoke: SUCCESS" : "MobileGlassEdgeSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Mobile Glass Edge Smoke Test");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine($"MobileGlassEdgeSmoke: FAILED ({ex.Message})");
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }
        finally
        {
            await server.StopAsync(CancellationToken.None);
        }
    }

    private static async Task<bool> StaticMobileGlassEdgePreparedAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var css = await client.GetStringAsync("/tray.css", cancellationToken);
        var js = await client.GetStringAsync("/tray.js", cancellationToken);
        return css.Contains(".glass-edge", StringComparison.Ordinal) &&
            css.Contains("is-single-glass-edge", StringComparison.Ordinal) &&
            js.Contains("activateMobileGlassEdgeMode", StringComparison.Ordinal) &&
            js.Contains("mobile/glass-edge-gesture", StringComparison.Ordinal) &&
            js.Contains("navigator.vibrate", StringComparison.Ordinal);
    }

    private static async Task<bool> VisibleLanguageIsHumanAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var text = await client.GetStringAsync("/mobile", cancellationToken);
        var forbidden = new[]
        {
            "Transfer",
            "Upload",
            "Download",
            "Senden",
            "Empfangen",
            "Sync",
            "Server",
            "Client",
            "Endpoint",
            "Device",
            "Geraet",
            "Gerät",
            "Agent",
            "Workspace",
            "IPC",
            "Request",
            "Response"
        };
        return forbidden.All(word => !text.Contains(word, StringComparison.Ordinal));
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

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
