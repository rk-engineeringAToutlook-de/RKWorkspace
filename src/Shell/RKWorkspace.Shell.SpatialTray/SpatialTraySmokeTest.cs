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
            var tactileUiOk = await StaticTactileUiIsPreparedAsync(client, timeout.Token);

            var initialHandy = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            var tactileConfigOk = initialHandy.RootElement.GetProperty("motion").GetProperty("tiltSource").GetString() == "movement-vector" &&
                initialHandy.RootElement.GetProperty("motion").GetProperty("initialResistanceDistancePx").GetDouble() > 0 &&
                initialHandy.RootElement.GetProperty("motion").GetProperty("heldCompactScale").GetDouble() < 1 &&
                initialHandy.RootElement.GetProperty("motion").GetProperty("liftDepthPx").GetDouble() > 0;
            var wobbleReducedOk = initialHandy.RootElement.GetProperty("motion").GetProperty("wobbleAmplitude").GetDouble() <= 0.01 &&
                initialHandy.RootElement.GetProperty("motion").GetProperty("wobbleFrequency").GetDouble() <= 0.03;
            var softSnapOk = initialHandy.RootElement.GetProperty("motion").GetProperty("softSnapStrength").GetDouble() <= 0.15 &&
                initialHandy.RootElement.GetProperty("motion").GetProperty("snapMode").GetString() == "soft-invitation";
            var hapticsPreparedOk = initialHandy.RootElement.GetProperty("haptics").GetProperty("mobilePrepared").GetBoolean() &&
                initialHandy.RootElement.GetProperty("haptics").GetProperty("opticalPrepared").GetBoolean() &&
                initialHandy.RootElement.GetProperty("haptics").GetProperty("partialOcclusion").GetBoolean() &&
                initialHandy.RootElement.GetProperty("haptics").GetProperty("contactShadow").GetBoolean();
            var transitionPreparedOk = initialHandy.RootElement.GetProperty("transition").GetProperty("targetGhostBeforePlace").GetBoolean() &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("glideIntoBubble").GetBoolean() &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("targetPositioning").GetString() == "relative-on-ablage";
            var portalPreparedOk = initialHandy.RootElement.GetProperty("transition").GetProperty("portalPhases").GetBoolean() &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("portalTransition").GetBoolean() &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("objectEmerges").GetBoolean() &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("enteringProgress").GetDouble() > 0 &&
                initialHandy.RootElement.GetProperty("transition").GetProperty("emergingProgress").GetDouble() < 1;
            var roomHasAblagenOk = HasAblage(initialHandy.RootElement, "handy") &&
                HasAblage(initialHandy.RootElement, "monitor");
            var preparedAblagenOk = initialHandy.RootElement.GetProperty("ablagen").GetArrayLength() >= 5;
            var initialThingOk = Thing(initialHandy.RootElement).GetProperty("currentAblageId").GetString() == "handy" &&
                Thing(initialHandy.RootElement).GetProperty("currentState").GetString() == "RestingOnAblage";
            var bubblesOnHandyOk = initialHandy.RootElement.GetProperty("bubbles").GetArrayLength() >= 2;
            var distanceLanguageOk = !Bubble(initialHandy.RootElement, "beamer").GetProperty("nameReadable").GetBoolean() &&
                Bubble(initialHandy.RootElement, "monitor").GetProperty("nameReadable").GetBoolean();

            using var pickHandy = await PostJsonAsync(client, "/api/pick", new { carrierAblageId = "handy" }, timeout.Token);
            var pickRemovesFromHandyOk = Thing(pickHandy.RootElement).GetProperty("currentAblageId").ValueKind == JsonValueKind.Null &&
                pickHandy.RootElement.GetProperty("activeCarry").GetProperty("state").GetString() == "Picked" &&
                pickHandy.RootElement.GetProperty("activeCarry").GetProperty("sourceAblageId").GetString() == "handy";
            var digitalHandOk = pickHandy.RootElement.GetProperty("surfaceThing").GetProperty("isCarriedHere").GetBoolean() &&
                pickHandy.RootElement.GetProperty("surfaceThing").GetProperty("heldCompact").GetBoolean() &&
                pickHandy.RootElement.GetProperty("surfaceThing").GetProperty("partialOcclusion").GetBoolean() &&
                pickHandy.RootElement.GetProperty("surfaceThing").GetProperty("glidePhase").GetString() == "Held";
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
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("isPreviewHere").GetBoolean() &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("text").GetString()?.Contains("kommt an", StringComparison.Ordinal) == true;
            var targetGhostOk = monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("ghostVisible").GetBoolean() &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("glidePhase").GetString() == "ReadyToPlace" &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("displayScale").GetDouble() < 1;
            var portalTransition = approachMonitor.RootElement.GetProperty("portalTransition");
            var portalTransitionOk = portalTransition.ValueKind == JsonValueKind.Object &&
                portalTransition.GetProperty("transitionId").GetString()?.StartsWith("portal-", StringComparison.Ordinal) == true &&
                portalTransition.GetProperty("thingId").GetString() == "thing-rechnung" &&
                portalTransition.GetProperty("sourceAblageId").GetString() == "handy" &&
                portalTransition.GetProperty("targetAblageId").GetString() == "monitor" &&
                portalTransition.GetProperty("state").GetString() == "ReadyToPlace" &&
                portalTransition.GetProperty("progress").GetDouble() > 0 &&
                portalTransition.GetProperty("progress").GetDouble() < 1;
            var sourceProgressOk = approachMonitor.RootElement.GetProperty("surfaceThing").GetProperty("sourceVisualProgress").GetDouble() > 0 &&
                approachMonitor.RootElement.GetProperty("surfaceThing").GetProperty("glidePhase").GetString() == "ObjectEntering";
            var targetProgressOk = monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("targetVisualProgress").GetDouble() > 0 &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("displayScale").GetDouble() > 0.8;
            var noInstantJumpOk = Thing(approachMonitor.RootElement).GetProperty("currentAblageId").ValueKind == JsonValueKind.Null &&
                !monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean() &&
                Thing(approachMonitor.RootElement).GetProperty("currentState").GetString() == "PreviewOnAblage";
            var openingAblageOk = approachMonitor.RootElement.GetProperty("activeCarry").GetProperty("state").GetString() == "OpeningAblage" &&
                Bubble(approachMonitor.RootElement, "monitor").GetProperty("opens").GetBoolean() &&
                Bubble(approachMonitor.RootElement, "monitor").GetProperty("portalPhase").GetString() == "ObjectEmerging";
            var portalEdgeOk = Bubble(approachMonitor.RootElement, "monitor").GetProperty("x").GetDouble() > 0.85 &&
                Bubble(approachMonitor.RootElement, "monitor").GetProperty("isPortal").GetBoolean();
            var readyToPlaceOk = monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("readyToPlace").GetBoolean() &&
                monitorPreview.RootElement.GetProperty("surfaceThing").GetProperty("glidePhase").GetString() == "ReadyToPlace";
            var activeBubbleOk = Bubble(approachMonitor.RootElement, "monitor").GetProperty("actionText").GetString() == "Hier ablegen";

            using var placeMonitor = await PostJsonAsync(
                client,
                "/api/place",
                new { carrierAblageId = "handy", targetAblageId = "monitor", x = 0.64, y = 0.42 },
                timeout.Token);
            using var handyAfterMonitorPlace = await GetJsonAsync(client, "/api/state?ablage=handy", timeout.Token);
            using var monitorAfterPlace = await GetJsonAsync(client, "/api/state?ablage=monitor", timeout.Token);
            var placeMonitorOk = Thing(placeMonitor.RootElement).GetProperty("currentAblageId").GetString() == "monitor" &&
                Thing(placeMonitor.RootElement).GetProperty("currentState").GetString() == "PlacedOnAblage" &&
                monitorAfterPlace.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean() &&
                !handyAfterMonitorPlace.RootElement.GetProperty("surfaceThing").GetProperty("isHere").GetBoolean();
            var targetPositionOk = Thing(placeMonitor.RootElement).GetProperty("positionOnAblage").GetProperty("x").GetDouble() == 0.64 &&
                Thing(placeMonitor.RootElement).GetProperty("positionOnAblage").GetProperty("y").GetDouble() == 0.42;

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
            var returnPortalOk = approachHandy.RootElement.GetProperty("portalTransition").GetProperty("sourceAblageId").GetString() == "monitor" &&
                approachHandy.RootElement.GetProperty("portalTransition").GetProperty("targetAblageId").GetString() == "handy";

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
                tactileUiOk &&
                tactileConfigOk &&
                wobbleReducedOk &&
                softSnapOk &&
                hapticsPreparedOk &&
                transitionPreparedOk &&
                portalPreparedOk &&
                roomHasAblagenOk &&
                preparedAblagenOk &&
                initialThingOk &&
                pickRemovesFromHandyOk &&
                digitalHandOk &&
                sourceTraceOk &&
                monitorPreviewOk &&
                targetGhostOk &&
                portalTransitionOk &&
                sourceProgressOk &&
                targetProgressOk &&
                noInstantJumpOk &&
                openingAblageOk &&
                portalEdgeOk &&
                readyToPlaceOk &&
                activeBubbleOk &&
                placeMonitorOk &&
                targetPositionOk &&
                pickFromMonitorOk &&
                handyPreviewOk &&
                returnPortalOk &&
                placeHandyOk &&
                freePlaceOk &&
                cancelOk &&
                languageOk &&
                distanceLanguageOk &&
                bubblesOnHandyOk;

            Console.WriteLine("Spatial Room Smoke Test");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Health: {(healthOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Handy surface: {(handySurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Monitor surface: {(monitorSurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Tactile UI: {(tactileUiOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Tactile config: {(tactileConfigOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Wobble reduced: {(wobbleReducedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Soft snap: {(softSnapOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Haptics prepared: {(hapticsPreparedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Glide prepared: {(transitionPreparedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Portal prepared: {(portalPreparedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Room ablagen: {(roomHasAblagenOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Prepared ablagen: {(preparedAblagenOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Initial thing: {(initialThingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Pick removes source: {(pickRemovesFromHandyOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Digital hand: {(digitalHandOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Source trace: {(sourceTraceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Monitor preview: {(monitorPreviewOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Target ghost: {(targetGhostOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Portal transition: {(portalTransitionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Source progress: {(sourceProgressOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Target progress: {(targetProgressOk ? "OK" : "FAILED")}");
            Console.WriteLine($"No instant jump: {(noInstantJumpOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Opening ablage: {(openingAblageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Portal edge: {(portalEdgeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Ready to place: {(readyToPlaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Active bubble: {(activeBubbleOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Place on monitor: {(placeMonitorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Target position: {(targetPositionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Pick from monitor: {(pickFromMonitorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Handy preview: {(handyPreviewOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Return portal: {(returnPortalOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Place on handy: {(placeHandyOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Free place: {(freePlaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Cancel return: {(cancelOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Human words: {(languageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"Bubble distance: {(distanceLanguageOk ? "OK" : "FAILED")}");
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
            "Portal-API",
            "Request",
            "Response",
            "Empfaenger",
            "Empfänger",
            "Sender"
        };
        return forbidden.All(word => !text.Contains(word, StringComparison.Ordinal));
    }

    private static async Task<bool> StaticTactileUiIsPreparedAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var css = await client.GetStringAsync("/tray.css", cancellationToken);
        var js = await client.GetStringAsync("/tray.js", cancellationToken);
        return css.Contains("rotateX", StringComparison.Ordinal) &&
            css.Contains("rotateY", StringComparison.Ordinal) &&
            css.Contains("is-gliding-into-bubble", StringComparison.Ordinal) &&
            css.Contains("is-occluded", StringComparison.Ordinal) &&
            css.Contains("bubble-lens", StringComparison.Ordinal) &&
            css.Contains("is-portal", StringComparison.Ordinal) &&
            css.Contains("is-entering", StringComparison.Ordinal) &&
            css.Contains("is-emerging", StringComparison.Ordinal) &&
            css.Contains("--portal-progress", StringComparison.Ordinal) &&
            js.Contains("targetTilt", StringComparison.Ordinal) &&
            js.Contains("movement.x", StringComparison.Ordinal) &&
            js.Contains("movement.y", StringComparison.Ordinal) &&
            js.Contains("portalProgress", StringComparison.Ordinal) &&
            js.Contains("is-ready-to-place", StringComparison.Ordinal) &&
            js.Contains("navigator.vibrate", StringComparison.Ordinal) &&
            js.Contains("Ding gleitet hinein", StringComparison.Ordinal);
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

    private static JsonElement Bubble(JsonElement state, string ablageId)
    {
        return state.GetProperty("bubbles")
            .EnumerateArray()
            .Single(ablage => ablage.GetProperty("ablageId").GetString() == ablageId);
    }

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
