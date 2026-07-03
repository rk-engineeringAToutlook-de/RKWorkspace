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
            var ablageOk = await GetContainsAsync(client, "/ablage", "Ablage Monitor bereit", timeout.Token);
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

            var success = healthOk && trayOk && ablageOk && demoThingOk && placeOk;

            Console.WriteLine("Spatial Tray Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Server: {(healthOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Tray endpoint: {(trayOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Ablage endpoint: {(ablageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Demo thing: {(demoThingOk ? "OK" : "FAILED")}");
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

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
