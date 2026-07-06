using RKWorkspace.Shell;

var options = ManualMapToolOptions.Parse(args, FindRoot());
var store = new ManualAblageMapStore(options.ConfigPath);

try
{
    if (options.ShowHelp)
    {
        PrintHelp();
        return 0;
    }

    if (options.SmokeTest)
    {
        return RunSmokeTest(options.Root);
    }

    if (options.Clear)
    {
        store.Clear();
        PrintHeader("Clear", store.Path);
        Console.WriteLine("ManualMapClear: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    if (options.Set)
    {
        if (string.IsNullOrWhiteSpace(options.Ablage) ||
            string.IsNullOrWhiteSpace(options.Direction) ||
            string.IsNullOrWhiteSpace(options.Distance))
        {
            throw new ArgumentException("-Set requires -Ablage, -Direction and -Distance.");
        }

        var entry = CreateEntry(
            options.Ablage,
            ParseRequiredEnum<AblageDirection>(options.Direction, nameof(options.Direction)),
            ParseRequiredEnum<AblageDistanceKind>(options.Distance, nameof(options.Distance)),
            options.DistanceMeters,
            options.Confidence);
        var map = store.Upsert(entry);
        PrintHeader("Set", store.Path);
        PrintEntry(entry);
        PrintNearest(map);
        Console.WriteLine("ManualMapSet: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    PrintHeader("List", store.Path);
    var loaded = store.Load();
    PrintMap(loaded);
    if (loaded.Entries.Count > 0)
    {
        PrintNearest(loaded);
    }

    Console.WriteLine("ManualMapList: SUCCESS");
    Console.WriteLine("RESULT: SUCCESS");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace Manual Map Tool");
    Console.WriteLine("----------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static int RunSmokeTest(string root)
{
    var smokePath = Path.Combine(root, "logs", "manual-map", "manual-ablage-map-smoke.json");
    var store = new ManualAblageMapStore(smokePath);
    store.Clear();

    var mac = CreateEntry("macOS", AblageDirection.Right, AblageDistanceKind.Near, 1.10, 0.94);
    var ipad = CreateEntry("iPad", AblageDirection.Up, AblageDistanceKind.Medium, 2.35, 0.82);
    var iphone = CreateEntry("iPhone", AblageDirection.Down, AblageDistanceKind.Far, 3.45, 0.76);
    store.Upsert(mac);
    store.Upsert(ipad);
    var saved = store.Upsert(iphone);
    var loaded = store.Load();
    var provider = new ManualMapAblageProximityProvider(loaded);
    var snapshot = provider.GetSnapshot(SimulatedAblageProximityProvider.WindowsAblageId);
    var nearest = new NearestAblageSelector().Select(snapshot);
    var edge = GlassEdge.FromNearest(nearest, GlassEdgeState.Visible, 0.65, 0.0);
    var invalidRejected = Throws(() => CreateEntry("Broken", AblageDirection.Unknown, AblageDistanceKind.Near, 1.0, 0.9));
    store.Clear();
    var clearWorks = !File.Exists(smokePath);

    var saveOk = saved.Entries.Count == 3;
    var loadOk = loaded.Entries.Count == 3;
    var nearestOk = nearest.HasTarget &&
        nearest.TargetAblageId?.Value == "ablage-macos" &&
        nearest.Source == AblageProximitySource.ManualMap;
    var directionOk = nearest.EdgeHint == AblageDirection.Right && edge.Direction == AblageDirection.Right;
    var distanceOk = nearest.Distance == AblageDistanceKind.Near;
    var singleOk = snapshot.AvailableTargets().Count(surface => surface.Id == nearest.TargetAblageId) == 1;

    var success = saveOk &&
        loadOk &&
        nearestOk &&
        directionOk &&
        distanceOk &&
        singleOk &&
        clearWorks &&
        invalidRejected;

    PrintHeader("SmokeTest", smokePath);
    Console.WriteLine($"ManualMapSave: {(saveOk ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapLoad: {(loadOk ? "OK" : "FAILED")}");
    Console.WriteLine($"NearestAblage: {nearest.TargetDisplayName}");
    Console.WriteLine($"NearestSource: {nearest.Source}");
    Console.WriteLine($"Direction: {nearest.EdgeHint}");
    Console.WriteLine($"Distance: {nearest.Distance}");
    Console.WriteLine($"SingleAblageSelected: {(singleOk ? "OK" : "FAILED")}");
    Console.WriteLine($"Clear: {(clearWorks ? "OK" : "FAILED")}");
    Console.WriteLine($"InvalidDirectionRejected: {(invalidRejected ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapSmoke: {(success ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RESULT: {(success ? "SUCCESS" : "FAILED")}");
    return success ? 0 : 1;
}

static void PrintHelp()
{
    Console.WriteLine("RK Workspace Manual Map Tool");
    Console.WriteLine("----------------------------");
    Console.WriteLine("Usage:");
    Console.WriteLine("  run-manual-map.ps1 -List");
    Console.WriteLine("  run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near");
    Console.WriteLine("  run-manual-map.ps1 -Clear");
    Console.WriteLine("  run-manual-map.ps1 -SmokeTest");
}

static void PrintHeader(string mode, string path)
{
    Console.WriteLine("RK Workspace Manual Map Tool");
    Console.WriteLine("----------------------------");
    Console.WriteLine($"Mode: {mode}");
    Console.WriteLine($"ConfigPath: {path}");
}

static void PrintMap(ManualAblageMap map)
{
    Console.WriteLine($"Entries: {map.Entries.Count}");
    foreach (var entry in map.Entries)
    {
        PrintEntry(entry);
    }
}

static void PrintEntry(ManualAblageMapEntry entry)
{
    Console.WriteLine($"AblageId: {entry.AblageId}");
    Console.WriteLine($"DisplayName: {entry.DisplayName}");
    Console.WriteLine($"Direction: {entry.RelativeDirection}");
    Console.WriteLine($"Distance: {entry.DistanceClass}");
    Console.WriteLine($"DistanceMeters: {(entry.DistanceMeters.HasValue ? entry.DistanceMeters.Value.ToString("0.00") : "unknown")}");
    Console.WriteLine($"Confidence: {entry.Confidence:0.00}");
    Console.WriteLine($"Available: {entry.IsAvailable}");
    Console.WriteLine($"Platform: {entry.Platform}");
}

static void PrintNearest(ManualAblageMap map)
{
    var provider = new AblageProximityProviderChain(
    [
        new ManualMapAblageProximityProvider(map),
        new SimulatedAblageProximityProvider()
    ]);
    var snapshot = provider.GetSnapshot(SimulatedAblageProximityProvider.WindowsAblageId);
    var nearest = new NearestAblageSelector().Select(snapshot);
    Console.WriteLine($"NearestAblage: {nearest.TargetDisplayName}");
    Console.WriteLine($"EdgeDirection: {nearest.EdgeHint}");
    Console.WriteLine($"NearestSource: {nearest.Source}");
}

static ManualAblageMapEntry CreateEntry(
    string ablage,
    AblageDirection direction,
    AblageDistanceKind distance,
    double? distanceMeters,
    double? confidence)
{
    var normalized = NormalizeAblage(ablage);
    var entry = new ManualAblageMapEntry(
        normalized.Id,
        normalized.DisplayName,
        direction,
        distance,
        distanceMeters ?? DefaultMeters(distance),
        confidence ?? 0.90,
        true,
        DateTimeOffset.UtcNow,
        AblageProximitySource.ManualMap,
        normalized.Platform);
    ManualAblageMapValidator.ValidateEntry(entry).ThrowIfInvalid();
    return entry;
}

static (string Id, string DisplayName, AblageSurfacePlatform Platform) NormalizeAblage(string value)
{
    var trimmed = value.Trim();
    var clean = trimmed
        .Replace(" ", "-", StringComparison.Ordinal)
        .Replace("_", "-", StringComparison.Ordinal)
        .ToLowerInvariant();
    var id = clean.StartsWith("ablage-", StringComparison.Ordinal)
        ? clean
        : $"ablage-{clean}";
    var displayName = trimmed.StartsWith("Ablage ", StringComparison.OrdinalIgnoreCase)
        ? trimmed
        : $"Ablage {trimmed}";

    var platform = clean switch
    {
        var text when text.Contains("mac", StringComparison.OrdinalIgnoreCase) => AblageSurfacePlatform.MacOS,
        var text when text.Contains("iphone", StringComparison.OrdinalIgnoreCase) ||
                      text.Contains("ipad", StringComparison.OrdinalIgnoreCase) ||
                      text.Contains("ios", StringComparison.OrdinalIgnoreCase) => AblageSurfacePlatform.IOS,
        var text when text.Contains("android", StringComparison.OrdinalIgnoreCase) => AblageSurfacePlatform.Android,
        var text when text.Contains("windows", StringComparison.OrdinalIgnoreCase) => AblageSurfacePlatform.Windows,
        _ => AblageSurfacePlatform.Unknown
    };

    return (id, displayName, platform);
}

static TEnum ParseRequiredEnum<TEnum>(string value, string name)
    where TEnum : struct, Enum
{
    if (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ||
        !Enum.IsDefined(parsed) ||
        parsed.ToString() == "Unknown")
    {
        throw new ArgumentException($"Invalid {name}: {value}");
    }

    return parsed;
}

static double? DefaultMeters(AblageDistanceKind distance)
{
    return distance switch
    {
        AblageDistanceKind.VeryNear => 0.45,
        AblageDistanceKind.Near => 1.20,
        AblageDistanceKind.Medium => 2.60,
        AblageDistanceKind.Far => 4.20,
        AblageDistanceKind.VeryFar => 6.20,
        _ => null
    };
}

static bool Throws(Action action)
{
    try
    {
        action();
        return false;
    }
    catch
    {
        return true;
    }
}

static string FindRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new InvalidOperationException("Repository root could not be located.");
    }

    return directory.FullName;
}

public sealed record ManualMapToolOptions(
    string Root,
    string ConfigPath,
    bool List,
    bool Set,
    bool Clear,
    bool SmokeTest,
    bool ShowHelp,
    string? Ablage,
    string? Direction,
    string? Distance,
    double? DistanceMeters,
    double? Confidence)
{
    public static ManualMapToolOptions Parse(string[] args, string root)
    {
        var list = false;
        var set = false;
        var clear = false;
        var smokeTest = false;
        var showHelp = false;
        string? ablage = null;
        string? direction = null;
        string? distance = null;
        double? distanceMeters = null;
        double? confidence = null;
        var configPath = ManualAblageMapStore.DefaultPath(root);

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Matches(arg, "--list", "-List"))
            {
                list = true;
                continue;
            }

            if (Matches(arg, "--set", "-Set"))
            {
                set = true;
                continue;
            }

            if (Matches(arg, "--clear", "-Clear"))
            {
                clear = true;
                continue;
            }

            if (Matches(arg, "--smoke-test", "-SmokeTest"))
            {
                smokeTest = true;
                continue;
            }

            if (Matches(arg, "--help", "-Help", "-?"))
            {
                showHelp = true;
                continue;
            }

            if (Matches(arg, "--ablage", "-Ablage") && index + 1 < args.Length)
            {
                ablage = args[++index];
                continue;
            }

            if (Matches(arg, "--direction", "-Direction") && index + 1 < args.Length)
            {
                direction = args[++index];
                continue;
            }

            if (Matches(arg, "--distance", "-Distance") && index + 1 < args.Length)
            {
                distance = args[++index];
                continue;
            }

            if (Matches(arg, "--distance-meters", "-DistanceMeters") && index + 1 < args.Length)
            {
                distanceMeters = double.Parse(args[++index], System.Globalization.CultureInfo.InvariantCulture);
                continue;
            }

            if (Matches(arg, "--confidence", "-Confidence") && index + 1 < args.Length)
            {
                confidence = double.Parse(args[++index], System.Globalization.CultureInfo.InvariantCulture);
                continue;
            }

            if (Matches(arg, "--config-path", "-ConfigPath") && index + 1 < args.Length)
            {
                configPath = Path.GetFullPath(args[++index]);
            }
        }

        if (!list && !set && !clear && !smokeTest && !showHelp)
        {
            list = true;
        }

        return new ManualMapToolOptions(
            root,
            configPath,
            list,
            set,
            clear,
            smokeTest,
            showHelp,
            ablage,
            direction,
            distance,
            distanceMeters,
            confidence);
    }

    private static bool Matches(string value, params string[] candidates)
    {
        return candidates.Any(candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
    }
}
