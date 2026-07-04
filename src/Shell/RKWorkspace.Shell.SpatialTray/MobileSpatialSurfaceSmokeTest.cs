using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RKWorkspace.Shell.SpatialTray;

public static class MobileSpatialSurfaceSmokeTest
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
            var initial = await GetJsonAsync(client, "/api/state?ablage=tablet", timeout.Token);
            var lensesHiddenBeforeGestureOk = !initial.RootElement.GetProperty("bubblesVisible").GetBoolean();
            var humanLanguageOk = await VisibleLanguageIsHumanAsync(client, timeout.Token);
            var staticMobileOk = await StaticMobileUiIsPreparedAsync(client, timeout.Token);
            using var gesture = await PostJsonAsync(client, "/api/mobile/gesture", new { gesture = "long-touch" }, timeout.Token);
            var root = gesture.RootElement;
            var modeOk = root.GetProperty("mobileSpatialMode").GetString() == "Active";
            var lensesVisibleOk = root.GetProperty("lensesVisible").GetBoolean();
            var lenses = root.GetProperty("lenses").EnumerateArray().ToArray();
            var lensCountOk = lenses.Length >= 2;
            var near = lenses.Single(lens => lens.GetProperty("ablageId").GetString() == "monitor");
            var far = lenses.Single(lens => lens.GetProperty("ablageId").GetString() == "beamer");
            var distanceScalingOk = near.GetProperty("scale").GetDouble() > far.GetProperty("scale").GetDouble() &&
                near.GetProperty("opacity").GetDouble() > far.GetProperty("opacity").GetDouble();
            var nameRevealOk = near.GetProperty("nameVisible").GetBoolean() &&
                !far.GetProperty("nameVisible").GetBoolean();
            var lensOpeningOk = near.GetProperty("opens").GetBoolean() &&
                near.GetProperty("isPortal").GetBoolean() &&
                near.GetProperty("actionText").GetString() == "Hier ablegen";
            var hapticsOk = root.GetProperty("haptics").GetProperty("requested").GetBoolean() &&
                root.GetProperty("haptics").GetProperty("skippedSafely").GetBoolean();
            var noTechnicalWordsOk = !root.GetProperty("forbiddenWordsVisible").GetBoolean() && humanLanguageOk;
            var success = mobileSurfaceOk &&
                lensesHiddenBeforeGestureOk &&
                staticMobileOk &&
                modeOk &&
                lensesVisibleOk &&
                lensCountOk &&
                distanceScalingOk &&
                nameRevealOk &&
                lensOpeningOk &&
                hapticsOk &&
                noTechnicalWordsOk;

            Console.WriteLine("RK Workspace Mobile Spatial Surface Smoke Test");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"MobileSurface: {(mobileSurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"GestureMode: {(staticMobileOk ? "OK" : "FAILED")}");
            Console.WriteLine($"LensesHiddenBeforeGesture: {(lensesHiddenBeforeGestureOk ? "OK" : "FAILED")}");
            Console.WriteLine($"MobileSpatialMode: {(modeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"LensesAfterGesture: {(lensesVisibleOk && lensCountOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DistanceScaling: {(distanceScalingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NameReveal: {(nameRevealOk ? "OK" : "FAILED")}");
            Console.WriteLine($"LensOpening: {(lensOpeningOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Haptics: {(hapticsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NoTechnicalWords: {(noTechnicalWordsOk ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "MobileSpatialSurfaceSmoke: SUCCESS" : "MobileSpatialSurfaceSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Mobile Spatial Surface Smoke Test");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"MobileSurface: FAILED ({ex.Message})");
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

    private static async Task<bool> StaticMobileUiIsPreparedAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var css = await client.GetStringAsync("/tray.css", cancellationToken);
        var js = await client.GetStringAsync("/tray.js", cancellationToken);
        var manifest = await client.GetStringAsync("/manifest.webmanifest", cancellationToken);
        return css.Contains(".is-mobile-spatial-active", StringComparison.Ordinal) &&
            css.Contains("mobile-spatial-gesture", StringComparison.Ordinal) &&
            css.Contains("radial-gradient", StringComparison.Ordinal) &&
            js.Contains("activateMobileSpatialMode", StringComparison.Ordinal) &&
            js.Contains("mobileSpatialMode", StringComparison.Ordinal) &&
            js.Contains("long-touch", StringComparison.Ordinal) &&
            js.Contains("navigator.vibrate", StringComparison.Ordinal) &&
            manifest.Contains("\"display\": \"standalone\"", StringComparison.Ordinal);
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

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
