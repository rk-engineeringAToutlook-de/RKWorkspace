using System.Globalization;
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

    if (!string.IsNullOrWhiteSpace(options.ImportPath))
    {
        var imported = ImportMap(options.ImportPath);
        var report = ValidateMapForSelector(imported);
        if (!report.IsValid)
        {
            PrintHeader("Import", store.Path);
            PrintValidation(report);
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }

        store.Save(imported);
        PrintHeader("Import", store.Path);
        PrintMap(imported);
        PrintValidation(report);
        Console.WriteLine("ManualMapImport: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    if (!string.IsNullOrWhiteSpace(options.ExportPath))
    {
        var map = store.Load();
        var report = ValidateMapForSelector(map);
        if (!report.IsValid)
        {
            PrintHeader("Export", store.Path);
            PrintValidation(report);
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }

        ExportMap(map, options.ExportPath);
        PrintHeader("Export", store.Path);
        Console.WriteLine($"ExportPath: {Path.GetFullPath(options.ExportPath)}");
        PrintValidation(report);
        Console.WriteLine("ManualMapExport: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    if (options.Remove)
    {
        if (string.IsNullOrWhiteSpace(options.Ablage))
        {
            throw new ArgumentException("-Remove requires -Ablage.");
        }

        var normalized = NormalizeAblage(options.Ablage);
        var map = store.Remove(normalized.Id);
        PrintHeader("Remove", store.Path);
        Console.WriteLine($"RemovedAblageId: {normalized.Id}");
        PrintMap(map);
        Console.WriteLine("ManualMapRemove: SUCCESS");
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

    if (options.Validate)
    {
        var map = store.Load();
        var report = ValidateMapForSelector(map);
        PrintHeader("Validate", store.Path);
        PrintValidation(report);
        Console.WriteLine($"RESULT: {(report.IsValid ? "SUCCESS" : "FAILED")}");
        return report.IsValid ? 0 : 1;
    }

    if (options.Show)
    {
        var map = store.Load();
        PrintHeader("Show", store.Path);
        PrintMap(map);
        PrintValidation(ValidateMapForSelector(map));
        Console.WriteLine("ManualMapShow: SUCCESS");
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
    var smokeDirectory = Path.Combine(root, "logs", "manual-map");
    var smokePath = Path.Combine(smokeDirectory, "manual-ablage-map-smoke.json");
    var exportPath = Path.Combine(smokeDirectory, "manual-ablage-map-export-smoke.json");
    var store = new ManualAblageMapStore(smokePath);
    store.Clear();
    if (File.Exists(exportPath))
    {
        File.Delete(exportPath);
    }

    var mac = CreateEntry("macOS", AblageDirection.Right, AblageDistanceKind.Near, 1.10, 0.94);
    var ipad = CreateEntry("iPad", AblageDirection.Up, AblageDistanceKind.Medium, 2.35, 0.82);
    var iphone = CreateEntry("iPhone", AblageDirection.Down, AblageDistanceKind.Near, 1.85, 0.86);
    var linux = CreateEntry("Linux", AblageDirection.Left, AblageDistanceKind.Far, 4.80, 0.72);

    store.Upsert(mac);
    store.Upsert(ipad);
    store.Upsert(iphone);
    var saved = store.Upsert(linux);
    var loaded = store.Load();
    var validation = ValidateMapForSelector(loaded);
    var nearest = validation.Nearest;
    var edge = validation.Edge;
    ExportMap(loaded, exportPath);
    var exported = File.Exists(exportPath) && new FileInfo(exportPath).Length > 0;
    store.Clear();
    store.Save(ImportMap(exportPath));
    var imported = store.Load();
    var importWorks = imported.Entries.Count == 4;
    var removed = store.Remove("ablage-linux");
    var removeWorks = removed.Entries.Count == 3 &&
        removed.Entries.All(entry => !string.Equals(entry.AblageId, "ablage-linux", StringComparison.OrdinalIgnoreCase));
    store.Clear();
    var clearWorks = !File.Exists(smokePath);

    var invalidNameRejected = Throws(() => CreateEntry("", AblageDirection.Right, AblageDistanceKind.Near, 1.0, 0.9));
    var invalidDirectionRejected = Throws(() => CreateEntry("Broken", AblageDirection.Unknown, AblageDistanceKind.Near, 1.0, 0.9));
    var invalidConfidenceRejected = Throws(() => CreateEntry("Broken", AblageDirection.Right, AblageDistanceKind.Near, 1.0, 1.4));
    var duplicateRejected = !ValidateMapForSelector(new ManualAblageMap([mac, mac])).IsValid;

    var setOk = saved.Entries.Count == 4;
    var listOk = loaded.Entries.Count == 4;
    var validateOk = validation.IsValid;
    var nearestOk = nearest?.HasTarget == true &&
        nearest.TargetAblageId?.Value == "ablage-macos" &&
        nearest.Source == AblageProximitySource.ManualMap;
    var directionOk = nearest?.EdgeHint == AblageDirection.Right && edge?.Direction == AblageDirection.Right;
    var distanceOk = nearest?.Distance == AblageDistanceKind.Near;
    var selectorUsesMap = nearest?.Source == AblageProximitySource.ManualMap;
    var invalidRejected = invalidNameRejected &&
        invalidDirectionRejected &&
        invalidConfidenceRejected &&
        duplicateRejected;

    var success = setOk &&
        listOk &&
        exported &&
        importWorks &&
        removeWorks &&
        clearWorks &&
        validateOk &&
        nearestOk &&
        directionOk &&
        distanceOk &&
        selectorUsesMap &&
        invalidRejected;

    PrintHeader("SmokeTest", smokePath);
    Console.WriteLine($"ManualMapSet: {(setOk ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapList: {(listOk ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapExport: {(exported ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapImport: {(importWorks ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapRemove: {(removeWorks ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapClear: {(clearWorks ? "OK" : "FAILED")}");
    Console.WriteLine($"ManualMapValidate: {(validateOk ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"SelectorUsesMap: {(selectorUsesMap ? "OK" : "FAILED")}");
    Console.WriteLine($"NearestAblage: {nearest?.TargetDisplayName ?? "none"}");
    Console.WriteLine($"NearestSource: {nearest?.Source.ToString() ?? "Unknown"}");
    Console.WriteLine($"EdgeDirection: {nearest?.EdgeHint.ToString() ?? "Unknown"}");
    Console.WriteLine($"Distance: {nearest?.Distance.ToString() ?? "Unknown"}");
    Console.WriteLine($"InvalidValuesRejected: {(invalidRejected ? "OK" : "FAILED")}");
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
    Console.WriteLine("  run-manual-map.ps1 -Show");
    Console.WriteLine("  run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near -Confidence 0.9");
    Console.WriteLine("  run-manual-map.ps1 -Remove -Ablage macOS");
    Console.WriteLine("  run-manual-map.ps1 -Clear");
    Console.WriteLine("  run-manual-map.ps1 -Import config/samples/manual-ablage-map.sample.json");
    Console.WriteLine("  run-manual-map.ps1 -Export config/manual-map-lab.json");
    Console.WriteLine("  run-manual-map.ps1 -Validate");
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
    Console.WriteLine($"DistanceMeters: {(entry.DistanceMeters.HasValue ? entry.DistanceMeters.Value.ToString("0.00", CultureInfo.InvariantCulture) : "unknown")}");
    Console.WriteLine($"Confidence: {entry.Confidence.ToString("0.00", CultureInfo.InvariantCulture)}");
    Console.WriteLine($"Available: {entry.IsAvailable}");
    Console.WriteLine($"Platform: {entry.Platform}");
}

static void PrintNearest(ManualAblageMap map)
{
    var report = ValidateMapForSelector(map);
    if (report.Nearest is null)
    {
        Console.WriteLine("NearestAblage: none");
        Console.WriteLine("EdgeDirection: Unknown");
        Console.WriteLine("NearestSource: Unknown");
        return;
    }

    Console.WriteLine($"NearestAblage: {report.Nearest.TargetDisplayName}");
    Console.WriteLine($"EdgeDirection: {report.Nearest.EdgeHint}");
    Console.WriteLine($"NearestSource: {report.Nearest.Source}");
}

static void PrintValidation(ManualMapToolValidationReport report)
{
    Console.WriteLine($"ManualMapValidate: {(report.IsValid ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"SelectorReady: {(report.Nearest?.HasTarget == true ? "OK" : "FAILED")}");
    if (report.Nearest is not null)
    {
        Console.WriteLine($"NearestAblage: {report.Nearest.TargetDisplayName}");
        Console.WriteLine($"EdgeDirection: {report.Nearest.EdgeHint}");
        Console.WriteLine($"NearestSource: {report.Nearest.Source}");
    }

    foreach (var error in report.Errors)
    {
        Console.WriteLine($"ValidationError: {error}");
    }
}

static ManualMapToolValidationReport ValidateMapForSelector(ManualAblageMap map)
{
    var errors = ManualAblageMapValidator.Validate(map).Errors.ToList();
    if (map.Entries.Count == 0)
    {
        errors.Add("At least one target ablage is required.");
    }

    if (map.Entries.All(entry => !entry.IsAvailable))
    {
        errors.Add("At least one available target ablage is required.");
    }

    if (errors.Count > 0)
    {
        return new ManualMapToolValidationReport(false, errors, null, null);
    }

    try
    {
        var provider = new AblageProximityProviderChain(
        [
            new ManualMapAblageProximityProvider(map),
            new SimulatedAblageProximityProvider()
        ]);
        var snapshot = provider.GetSnapshot(SimulatedAblageProximityProvider.WindowsAblageId);
        var nearest = new NearestAblageSelector().Select(snapshot);
        if (!nearest.HasTarget)
        {
            errors.Add("NearestAblageSelector found no target.");
            return new ManualMapToolValidationReport(false, errors, nearest, null);
        }

        var edge = GlassEdge.FromNearest(nearest, GlassEdgeState.Visible, 0.65, 0.0);
        return new ManualMapToolValidationReport(true, [], nearest, edge);
    }
    catch (Exception ex)
    {
        errors.Add(ex.Message);
        return new ManualMapToolValidationReport(false, errors, null, null);
    }
}

static ManualAblageMap ImportMap(string path)
{
    var fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
        throw new FileNotFoundException("Import file not found.", fullPath);
    }

    return ManualAblageMapSerializer.Load(fullPath);
}

static void ExportMap(ManualAblageMap map, string path)
{
    var fullPath = Path.GetFullPath(path);
    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(fullPath, ManualAblageMapSerializer.Serialize(map));
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
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new ArgumentException("AblageId or name is required.");
    }

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
        var text when text.Contains("linux", StringComparison.OrdinalIgnoreCase) => AblageSurfacePlatform.Linux,
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

public sealed record ManualMapToolValidationReport(
    bool IsValid,
    IReadOnlyList<string> Errors,
    NearestAblageResult? Nearest,
    GlassEdge? Edge);

public sealed record ManualMapToolOptions(
    string Root,
    string ConfigPath,
    bool List,
    bool Show,
    bool Set,
    bool Remove,
    bool Clear,
    bool Validate,
    bool SmokeTest,
    bool ShowHelp,
    string? ImportPath,
    string? ExportPath,
    string? Ablage,
    string? Direction,
    string? Distance,
    double? DistanceMeters,
    double? Confidence)
{
    public static ManualMapToolOptions Parse(string[] args, string root)
    {
        var list = false;
        var show = false;
        var set = false;
        var remove = false;
        var clear = false;
        var validate = false;
        var smokeTest = false;
        var showHelp = false;
        string? importPath = null;
        string? exportPath = null;
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

            if (Matches(arg, "--show", "-Show"))
            {
                show = true;
                continue;
            }

            if (Matches(arg, "--set", "-Set"))
            {
                set = true;
                continue;
            }

            if (Matches(arg, "--remove", "-Remove"))
            {
                remove = true;
                continue;
            }

            if (Matches(arg, "--clear", "-Clear"))
            {
                clear = true;
                continue;
            }

            if (Matches(arg, "--validate", "-Validate"))
            {
                validate = true;
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

            if (Matches(arg, "--import", "-Import") && index + 1 < args.Length)
            {
                importPath = args[++index];
                continue;
            }

            if (Matches(arg, "--export", "-Export") && index + 1 < args.Length)
            {
                exportPath = args[++index];
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
                distanceMeters = double.Parse(args[++index], CultureInfo.InvariantCulture);
                continue;
            }

            if (Matches(arg, "--confidence", "-Confidence") && index + 1 < args.Length)
            {
                confidence = double.Parse(args[++index], CultureInfo.InvariantCulture);
                continue;
            }

            if (Matches(arg, "--config-path", "-ConfigPath") && index + 1 < args.Length)
            {
                configPath = Path.GetFullPath(args[++index]);
            }
        }

        if (!list &&
            !show &&
            !set &&
            !remove &&
            !clear &&
            !validate &&
            !smokeTest &&
            !showHelp &&
            string.IsNullOrWhiteSpace(importPath) &&
            string.IsNullOrWhiteSpace(exportPath))
        {
            list = true;
        }

        return new ManualMapToolOptions(
            root,
            configPath,
            list,
            show,
            set,
            remove,
            clear,
            validate,
            smokeTest,
            showHelp,
            importPath,
            exportPath,
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
