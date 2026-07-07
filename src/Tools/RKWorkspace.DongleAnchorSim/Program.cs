using System.Globalization;
using RKWorkspace.Shell;

var options = DongleAnchorSimOptions.Parse(args);

if (options.Help)
{
    PrintHelp();
    return 0;
}

var current = new AblageIdentity("ablage-windows");
var dongle = new SimulatedDongleAnchorProvider(new DongleAnchorSimulationOptions(
    options.DongleId,
    options.Profile,
    options.Status,
    options.SupportsBle,
    options.SupportsUwb,
    "EphemeralLab"));

var provider = options.UseFusion
    ? new ProximityFusionProvider(
        [
            dongle,
            new ManualMapAblageProximityProvider(CreateManualFallbackMap()),
            new SimulatedAblageProximityProvider()
        ],
        new ProximityFusionSettings { ConfidenceThreshold = options.ConfidenceThreshold }) as IAblageProximityProvider
    : dongle;

var snapshot = provider.GetSnapshot(current);
var nearest = new NearestAblageSelector(new NearestAblageSelectionSettings
{
    MinimumConfidence = options.ConfidenceThreshold
}).Select(snapshot);
var readings = dongle.GetAnchorReadings(current).Where(reading => reading.Surface.Id != current).ToArray();

PrintReport(options, dongle, readings, nearest);

var success =
    readings.Length >= 2 &&
    nearest.HasTarget &&
    nearest.Confidence >= options.ConfidenceThreshold &&
    nearest.EdgeHint != AblageDirection.Unknown &&
    dongle.SupportsBlePresence &&
    dongle.SupportsUwbRanging &&
    dongle.PrivacyMode == "EphemeralLab";

if (options.SmokeTest && !success)
{
    Console.WriteLine("DongleAnchorSmoke: FAILED");
    Console.WriteLine("RESULT: FAILED");
    return 1;
}

Console.WriteLine("DongleAnchorSmoke: SUCCESS");
Console.WriteLine("RESULT: SUCCESS");
return 0;

static ManualAblageMap CreateManualFallbackMap()
{
    var now = DateTimeOffset.UtcNow;
    return new ManualAblageMap(
    [
        new ManualAblageMapEntry(
            "ablage-dongle-ipad",
            "Ablage iPad via Manual Map",
            AblageDirection.Right,
            AblageDistanceKind.Near,
            0.74,
            0.88,
            true,
            now.AddMilliseconds(-120),
            AblageProximitySource.ManualMap,
            AblageSurfacePlatform.IOS),
        new ManualAblageMapEntry(
            "ablage-dongle-macos",
            "Ablage macOS via Manual Map",
            AblageDirection.UpRight,
            AblageDistanceKind.Near,
            1.20,
            0.86,
            true,
            now.AddMilliseconds(-140),
            AblageProximitySource.ManualMap,
            AblageSurfacePlatform.MacOS)
    ]);
}

static void PrintReport(
    DongleAnchorSimOptions options,
    SimulatedDongleAnchorProvider dongle,
    IReadOnlyList<DongleAnchorReading> readings,
    NearestAblageResult nearest)
{
    Console.WriteLine("RK Workspace Dongle Anchor Simulation");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"Profile: {options.Profile}");
    Console.WriteLine($"UseFusion: {(options.UseFusion ? "YES" : "NO")}");
    Console.WriteLine($"Provider: {nameof(SimulatedDongleAnchorProvider)}");
    Console.WriteLine($"DongleId: {dongle.DongleId}");
    Console.WriteLine($"ProviderStatus: {dongle.Status}");
    Console.WriteLine($"PrivacyMode: {dongle.PrivacyMode}");
    Console.WriteLine($"BleAnchor: {(dongle.SupportsBlePresence ? "OK" : "OFF")}");
    Console.WriteLine($"UwbAnchor: {(dongle.SupportsUwbRanging ? "OK" : "OFF")}");
    Console.WriteLine($"AnchorCount: {readings.Count}");

    foreach (var reading in readings)
    {
        Console.WriteLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"AnchorReading: {reading.Surface.DisplayName}; Direction={reading.Surface.Pose.Direction}; DistanceMeters={reading.Surface.Distance.DistanceMeters:0.00}; Confidence={reading.Surface.Distance.Confidence:0.00}; Beacon={reading.EphemeralBeaconId}"));
    }

    Console.WriteLine($"NearestAblage: {nearest.TargetDisplayName}");
    Console.WriteLine($"EdgeDirection: {nearest.EdgeHint}");
    Console.WriteLine($"DistanceKind: {nearest.Distance}");
    Console.WriteLine($"DistanceMeters: {nearest.DistanceMeters:0.00}");
    Console.WriteLine($"Confidence: {nearest.Confidence:0.00}");
    Console.WriteLine($"Source: {nearest.Source}");
    Console.WriteLine($"ManualMapFusion: {(options.UseFusion && nearest.Source == AblageProximitySource.SensorFusion ? "OK" : "NOT USED")}");
    Console.WriteLine($"DirectionDistanceConfidence: {(nearest.HasTarget && nearest.EdgeHint != AblageDirection.Unknown && nearest.Confidence >= options.ConfidenceThreshold ? "OK" : "FAILED")}");
    Console.WriteLine("DongleAnchorCli: SUCCESS");
}

static void PrintHelp()
{
    Console.WriteLine("RK Workspace Dongle Anchor Simulation");
    Console.WriteLine("Options:");
    Console.WriteLine("  --smoke-test");
    Console.WriteLine("  --profile Static|MovingCloser|MovingAway|PassingBy|NoisySignal");
    Console.WriteLine("  --use-fusion");
    Console.WriteLine("  --confidence-threshold 0.70");
}

public sealed record DongleAnchorSimOptions(
    bool SmokeTest,
    bool Help,
    string DongleId,
    UwbSimulationProfile Profile,
    UwbProviderStatus Status,
    bool SupportsBle,
    bool SupportsUwb,
    bool UseFusion,
    double ConfidenceThreshold)
{
    public static DongleAnchorSimOptions Parse(string[] args)
    {
        var smokeTest = false;
        var help = false;
        var dongleId = "dongle-lab-anchor-01";
        var profile = UwbSimulationProfile.MovingCloser;
        var status = UwbProviderStatus.Simulated;
        var supportsBle = true;
        var supportsUwb = true;
        var useFusion = false;
        var confidenceThreshold = 0.70;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                smokeTest = true;
                continue;
            }

            if (Is(arg, "--help", "-Help", "-h"))
            {
                help = true;
                continue;
            }

            if (Is(arg, "--dongle-id", "-DongleId") && index + 1 < args.Length)
            {
                dongleId = args[++index];
                continue;
            }

            if (Is(arg, "--profile", "-Profile") && index + 1 < args.Length)
            {
                profile = Enum.Parse<UwbSimulationProfile>(args[++index], ignoreCase: true);
                continue;
            }

            if (Is(arg, "--status", "-Status") && index + 1 < args.Length)
            {
                status = Enum.Parse<UwbProviderStatus>(args[++index], ignoreCase: true);
                continue;
            }

            if (Is(arg, "--no-ble", "-NoBle"))
            {
                supportsBle = false;
                continue;
            }

            if (Is(arg, "--no-uwb", "-NoUwb"))
            {
                supportsUwb = false;
                continue;
            }

            if (Is(arg, "--use-fusion", "-UseFusion"))
            {
                useFusion = true;
                continue;
            }

            if (Is(arg, "--confidence-threshold", "-ConfidenceThreshold") && index + 1 < args.Length)
            {
                confidenceThreshold = Math.Clamp(
                    double.Parse(args[++index], CultureInfo.InvariantCulture),
                    0.0,
                    1.0);
            }
        }

        return new DongleAnchorSimOptions(
            smokeTest,
            help,
            dongleId,
            profile,
            status,
            supportsBle,
            supportsUwb,
            useFusion,
            confidenceThreshold);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
