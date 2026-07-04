using System.Net;
using System.Net.Sockets;

public static class Real3DLensSmokeTest
{
    public static async Task<int> RunAsync(int? requestedPort = null)
    {
        var port = requestedPort is > 0 ? requestedPort.Value : FindAvailablePort();
        await using var server = new Real3DLensServer(port);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        try
        {
            await server.StartAsync(timeout.Token);
            using var client = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}"),
                Timeout = TimeSpan.FromSeconds(5)
            };

            var health = await client.GetStringAsync("/health", timeout.Token);
            var html = await client.GetStringAsync("/real3d", timeout.Token);
            var script = await client.GetStringAsync("/real3d.js", timeout.Token);
            var css = await client.GetStringAsync("/real3d.css", timeout.Token);

            var healthOk = health.Contains("\"status\":\"OK\"", StringComparison.OrdinalIgnoreCase) &&
                health.Contains("real-3d-lens", StringComparison.OrdinalIgnoreCase);
            var webGlOk = script.Contains("WebGLRenderer", StringComparison.Ordinal);
            var threeOk = html.Contains("three.module.js", StringComparison.Ordinal) &&
                script.Contains("MeshPhysicalMaterial", StringComparison.Ordinal);
            var physicalGlassOk = script.Contains("transmission", StringComparison.Ordinal) &&
                script.Contains("thickness", StringComparison.Ordinal) &&
                script.Contains("ior", StringComparison.Ordinal);
            var environmentOk = script.Contains("PMREMGenerator", StringComparison.Ordinal) &&
                script.Contains("RoomEnvironment", StringComparison.Ordinal) &&
                script.Contains("ACESFilmicToneMapping", StringComparison.Ordinal);
            var shadowsOk = script.Contains("PCFSoftShadowMap", StringComparison.Ordinal) &&
                script.Contains("shadowMap.enabled = true", StringComparison.Ordinal);
            var tunnelOk = script.Contains("TubeGeometry", StringComparison.Ordinal) &&
                script.Contains("TorusGeometry", StringComparison.Ordinal);
            var desktopCaptureOk = script.Contains("getDisplayMedia", StringComparison.Ordinal) &&
                script.Contains("VideoTexture", StringComparison.Ordinal);
            var deformingObjectOk = script.Contains("deformCardGeometry", StringComparison.Ordinal) &&
                script.Contains("portalPull", StringComparison.Ordinal);
            var premiumUiOk = css.Contains("backdrop-filter", StringComparison.Ordinal) &&
                html.Contains("real3d-canvas", StringComparison.Ordinal);

            var success = healthOk &&
                webGlOk &&
                threeOk &&
                physicalGlassOk &&
                environmentOk &&
                shadowsOk &&
                tunnelOk &&
                desktopCaptureOk &&
                deformingObjectOk &&
                premiumUiOk;

            Console.WriteLine("RK Workspace Real3D Lens Smoke Test");
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Health: {(healthOk ? "OK" : "FAILED")}");
            Console.WriteLine($"WebGLRenderer: {(webGlOk ? "OK" : "FAILED")}");
            Console.WriteLine($"ThreeJS: {(threeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"PhysicalGlass: {(physicalGlassOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EnvironmentLighting: {(environmentOk ? "OK" : "FAILED")}");
            Console.WriteLine($"SoftShadows: {(shadowsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Real3DTunnel: {(tunnelOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DesktopLiveTexture: {(desktopCaptureOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DeformingDigitalThing: {(deformingObjectOk ? "OK" : "FAILED")}");
            Console.WriteLine($"PremiumUiShell: {(premiumUiOk ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "Real3DLensSmoke: SUCCESS" : "Real3DLensSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");
            return success ? 0 : 1;
        }
        finally
        {
            await server.StopAsync(CancellationToken.None);
        }
    }

    private static int FindAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
