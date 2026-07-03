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
            var trayOk = await GetContainsAsync(client, "/tray", "Digitales Ding", timeout.Token) &&
                await GetContainsAsync(client, "/tray", "Rechnung.pdf", timeout.Token);
            var stateBefore = await client.GetStringAsync("/api/state", timeout.Token);
            var demoThingOk = stateBefore.Contains("Rechnung.pdf", StringComparison.Ordinal) &&
                stateBefore.Contains("ThingOnTray", StringComparison.Ordinal);
            var initialState = JsonDocument.Parse(stateBefore);
            var bubblesOk = HasExpectedBubbles(initialState.RootElement);
            var motionOk = HasSoftMotionSettings(initialState.RootElement);
            var ablageOk = await GetContainsAsync(client, "/ablage", "Ablage Monitor bereit", timeout.Token);

            using var pickResponse = await PostJsonAsync(client, "/api/pick", new { }, timeout.Token);
            var carryStateOk = pickResponse.RootElement.GetProperty("carryState").GetString() == "Picked";

            using var releaseResponse = await PostJsonAsync(
                client,
                "/api/release",
                new { x = 0.42, y = 0.58, placeOnAblage = false },
                timeout.Token);
            var freePlaceOk = releaseResponse.RootElement.GetProperty("state").GetString() == "Placed" &&
                releaseResponse.RootElement.GetProperty("placement").GetProperty("kind").GetString() == "Free" &&
                releaseResponse.RootElement.GetProperty("activeAblage").GetString() == "Freier Raum";

            using var cancelResponse = await PostJsonAsync(client, "/api/cancel", new { }, timeout.Token);
            var cancelOk = cancelResponse.RootElement.GetProperty("carryState").GetString() == "OnTray" &&
                cancelResponse.RootElement.GetProperty("placement").GetProperty("kind").GetString() == "Tray";

            using var nearResponse = await PostJsonAsync(
                client,
                "/api/near",
                new { ablage = "monitor" },
                timeout.Token);
            var activeBubbleOk = HasActiveBubble(nearResponse.RootElement);

            using var content = new StringContent(
                JsonSerializer.Serialize(new { ablage = "Ablage Monitor" }),
                Encoding.UTF8,
                "application/json");
            using var response = await client.PostAsync("/api/place", content, timeout.Token);
            var placeResponse = await response.Content.ReadAsStringAsync(timeout.Token);
            var stateAfter = await client.GetStringAsync("/api/state", timeout.Token);
            var placeOk = response.IsSuccessStatusCode &&
                placeResponse.Contains("Placed", StringComparison.Ordinal) &&
                stateAfter.Contains("Placed", StringComparison.Ordinal) &&
                await GetContainsAsync(client, "/ablage", "Hier liegt jetzt: Rechnung.pdf", timeout.Token);

            var success = healthOk &&
                trayOk &&
                ablageOk &&
                demoThingOk &&
                carryStateOk &&
                freePlaceOk &&
                cancelOk &&
                bubblesOk &&
                activeBubbleOk &&
                motionOk &&
                placeOk;

            Console.WriteLine("Spatial Tray Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Server: {(healthOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Tray endpoint: {(trayOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Ablage endpoint: {(ablageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Demo thing: {(demoThingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Carry state: {(carryStateOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Free place: {(freePlaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Cancel return: {(cancelOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Ablage bubbles: {(bubblesOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Active bubble: {(activeBubbleOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Soft motion: {(motionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Simulated place: {(placeOk ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Spatial Tray Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Server: FAILED ({ex.Message})");
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

    private static bool HasExpectedBubbles(JsonElement state)
    {
        var bubbles = state.GetProperty("bubbles").EnumerateArray().ToArray();
        if (bubbles.Length < 5)
        {
            return false;
        }

        var distinctScales = bubbles
            .Select(bubble => bubble.GetProperty("scale").GetDouble())
            .Distinct()
            .Count();
        var farNamesHidden = bubbles
            .Where(bubble => bubble.GetProperty("distance").GetString() is "VeryFar" or "Far")
            .All(bubble => !bubble.GetProperty("nameReadable").GetBoolean());
        var nearNamesReadable = bubbles
            .Where(bubble => bubble.GetProperty("distance").GetString() is "Near" or "VeryNear")
            .All(bubble => bubble.GetProperty("nameReadable").GetBoolean());

        return distinctScales >= 3 && farNamesHidden && nearNamesReadable;
    }

    private static bool HasActiveBubble(JsonElement state)
    {
        return state.GetProperty("bubbles")
            .EnumerateArray()
            .Any(bubble =>
                bubble.GetProperty("state").GetString() == "Active" &&
                bubble.GetProperty("actionText").GetString() == "Hier ablegen");
    }

    private static bool HasSoftMotionSettings(JsonElement state)
    {
        var motion = state.GetProperty("motion");
        return motion.GetProperty("wobbleAmplitude").GetDouble() <= 0.1 &&
            motion.GetProperty("wobbleFrequency").GetDouble() <= 0.2 &&
            motion.GetProperty("softSnapStrength").GetDouble() is > 0 and < 0.5 &&
            motion.GetProperty("nameRevealThreshold").GetDouble() > 0 &&
            motion.GetProperty("activationThreshold").GetDouble() > motion.GetProperty("nameRevealThreshold").GetDouble();
    }

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
