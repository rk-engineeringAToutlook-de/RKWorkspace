using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.Shell;

public sealed record ManualAblageMap
{
    public IReadOnlyList<ManualAblageMapEntry> Entries { get; init; } = [];

    public ManualAblageMap()
    {
    }

    public ManualAblageMap(IReadOnlyList<ManualAblageMapEntry> entries)
    {
        Entries = entries;
    }

    public ManualAblageMap Upsert(ManualAblageMapEntry entry)
    {
        var entries = Entries
            .Where(existing => !string.Equals(existing.AblageId, entry.AblageId, StringComparison.OrdinalIgnoreCase))
            .Append(entry with { LastUpdated = entry.LastUpdated == default ? DateTimeOffset.UtcNow : entry.LastUpdated })
            .OrderBy(existing => existing.AblageId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new ManualAblageMap(entries);
    }

    public static ManualAblageMap Empty { get; } = new([]);
}

public sealed record ManualAblageMapEntry
{
    public string AblageId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public AblageDirection RelativeDirection { get; init; } = AblageDirection.Unknown;

    public AblageDistanceKind DistanceClass { get; init; } = AblageDistanceKind.Unknown;

    public double? DistanceMeters { get; init; }

    public double Confidence { get; init; } = 0.80;

    public bool IsAvailable { get; init; } = true;

    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;

    public AblageProximitySource Source { get; init; } = AblageProximitySource.ManualMap;

    public AblageSurfacePlatform Platform { get; init; } = AblageSurfacePlatform.Unknown;

    public ManualAblageMapEntry()
    {
    }

    public ManualAblageMapEntry(
        string ablageId,
        string displayName,
        AblageDirection relativeDirection,
        AblageDistanceKind distanceClass,
        double? distanceMeters,
        double confidence,
        bool isAvailable,
        DateTimeOffset lastUpdated,
        AblageProximitySource source,
        AblageSurfacePlatform platform = AblageSurfacePlatform.Unknown)
    {
        AblageId = ablageId;
        DisplayName = displayName;
        RelativeDirection = relativeDirection;
        DistanceClass = distanceClass;
        DistanceMeters = distanceMeters;
        Confidence = confidence;
        IsAvailable = isAvailable;
        LastUpdated = lastUpdated;
        Source = source;
        Platform = platform;
    }

    public AblageSurface ToSurface()
    {
        return new AblageSurface(
            new AblageIdentity(AblageId),
            DisplayName,
            Platform,
            IsAvailable,
            AblagePose.FromDirection(RelativeDirection),
            AblageDistance.FromSource(DistanceClass, DistanceMeters, Confidence, Source),
            LastUpdated);
    }
}

public sealed record ManualAblageMapValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ManualAblageMapValidationResult Success { get; } = new(true, []);

    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException(string.Join("; ", Errors));
        }
    }
}

public static class ManualAblageMapValidator
{
    public static ManualAblageMapValidationResult Validate(ManualAblageMap map)
    {
        var errors = new List<string>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in map.Entries)
        {
            errors.AddRange(ValidateEntry(entry).Errors);
            if (!string.IsNullOrWhiteSpace(entry.AblageId) && !ids.Add(entry.AblageId))
            {
                errors.Add($"Duplicate AblageId '{entry.AblageId}'.");
            }
        }

        return errors.Count == 0
            ? ManualAblageMapValidationResult.Success
            : new ManualAblageMapValidationResult(false, errors);
    }

    public static ManualAblageMapValidationResult ValidateEntry(ManualAblageMapEntry entry)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(entry.AblageId))
        {
            errors.Add("AblageId is required.");
        }

        if (string.IsNullOrWhiteSpace(entry.DisplayName))
        {
            errors.Add("DisplayName is required.");
        }

        if (!Enum.IsDefined(entry.RelativeDirection) || entry.RelativeDirection == AblageDirection.Unknown)
        {
            errors.Add("RelativeDirection must be Left, Right, Up, Down, a diagonal direction, Front, or Back.");
        }

        if (!Enum.IsDefined(entry.DistanceClass) || entry.DistanceClass == AblageDistanceKind.Unknown)
        {
            errors.Add("DistanceClass must be VeryNear, Near, Medium, Far, or VeryFar.");
        }

        if (entry.DistanceMeters < 0)
        {
            errors.Add("DistanceMeters must not be negative.");
        }

        if (entry.Confidence is < 0 or > 1)
        {
            errors.Add("Confidence must be between 0.0 and 1.0.");
        }

        return errors.Count == 0
            ? ManualAblageMapValidationResult.Success
            : new ManualAblageMapValidationResult(false, errors);
    }
}

public static class ManualAblageMapSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static ManualAblageMap Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ManualAblageMap>(json, Options) ?? ManualAblageMap.Empty;
    }

    public static string Serialize(ManualAblageMap map)
    {
        return JsonSerializer.Serialize(map, Options);
    }

    public static ManualAblageMap Load(string path)
    {
        if (!File.Exists(path))
        {
            return ManualAblageMap.Empty;
        }

        return Deserialize(File.ReadAllText(path));
    }

    public static void Save(string path, ManualAblageMap map)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(map));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

public sealed class ManualAblageMapStore
{
    public ManualAblageMapStore(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public static string DefaultPath(string repositoryRoot)
    {
        return System.IO.Path.Combine(repositoryRoot, "config", "manual-ablage-map.json");
    }

    public ManualAblageMap Load()
    {
        return ManualAblageMapSerializer.Load(Path);
    }

    public ManualAblageMap Save(ManualAblageMap map)
    {
        ManualAblageMapValidator.Validate(map).ThrowIfInvalid();
        ManualAblageMapSerializer.Save(Path, map);
        return map;
    }

    public ManualAblageMap Upsert(ManualAblageMapEntry entry)
    {
        ManualAblageMapValidator.ValidateEntry(entry).ThrowIfInvalid();
        var map = Load().Upsert(entry);
        return Save(map);
    }

    public void Clear()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}

public sealed class ManualMapAblageProximityProvider : IAblageProximityProvider
{
    private readonly ManualAblageMap _map;

    public ManualMapAblageProximityProvider(ManualAblageMap map)
    {
        ManualAblageMapValidator.Validate(map).ThrowIfInvalid();
        _map = map;
    }

    public ManualMapAblageProximityProvider(IReadOnlyList<ManualAblageMapEntry> entries)
        : this(new ManualAblageMap(entries))
    {
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var surfaces = _map.Entries.Select(entry => entry.ToSurface()).ToArray();
        if (surfaces.All(surface => surface.Id != currentAblageId))
        {
            var now = DateTimeOffset.UtcNow;
            surfaces = surfaces
                .Prepend(new AblageSurface(
                    currentAblageId,
                    "Aktuelle Ablage",
                    AblageSurfacePlatform.Windows,
                    true,
                    AblagePose.FromDirection(AblageDirection.Unknown),
                    AblageDistance.FromSource(AblageDistanceKind.VeryNear, 0, 1, AblageProximitySource.ManualMap),
                    now))
                .ToArray();
        }

        return new AblageProximitySnapshot(currentAblageId, surfaces, DateTimeOffset.UtcNow);
    }

    public static ManualAblageMap CreateOwnerRoomExample(DateTimeOffset? capturedAt = null)
    {
        var now = capturedAt ?? DateTimeOffset.UtcNow;
        return new ManualAblageMap(
        [
            new ManualAblageMapEntry(
                "ablage-macos",
                "Ablage macOS",
                AblageDirection.Right,
                AblageDistanceKind.Near,
                1.10,
                0.93,
                true,
                now.AddSeconds(-1),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.MacOS),
            new ManualAblageMapEntry(
                "ablage-ipad",
                "Ablage iPad",
                AblageDirection.Up,
                AblageDistanceKind.Medium,
                2.35,
                0.82,
                true,
                now.AddSeconds(-2),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.IOS),
            new ManualAblageMapEntry(
                "ablage-iphone",
                "Ablage iPhone",
                AblageDirection.Down,
                AblageDistanceKind.Far,
                3.45,
                0.78,
                true,
                now.AddSeconds(-3),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.IOS),
            new ManualAblageMapEntry(
                "ablage-monitor-links",
                "Ablage Monitor links",
                AblageDirection.Left,
                AblageDistanceKind.VeryFar,
                5.80,
                0.64,
                true,
                now.AddSeconds(-4),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.Unknown)
        ]);
    }
}

public sealed class AblageProximityProviderChain : IAblageProximityProvider
{
    private readonly IReadOnlyList<IAblageProximityProvider> _providers;

    public AblageProximityProviderChain(IReadOnlyList<IAblageProximityProvider> providers)
    {
        _providers = providers.Count > 0
            ? providers
            : throw new ArgumentException("At least one proximity provider is required.", nameof(providers));
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        AblageProximitySnapshot? fallback = null;
        foreach (var provider in _providers)
        {
            var snapshot = provider.GetSnapshot(currentAblageId);
            fallback ??= snapshot;
            if (snapshot.AvailableTargets().Any())
            {
                return snapshot;
            }
        }

        return fallback ?? new AblageProximitySnapshot(currentAblageId, [], DateTimeOffset.UtcNow);
    }
}
