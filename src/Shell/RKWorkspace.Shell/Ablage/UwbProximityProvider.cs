namespace RKWorkspace.Shell;

public enum UwbProviderStatus
{
    Unknown,
    Ready,
    Simulated,
    HardwareUnavailable,
    Degraded,
    ConsentRequired
}

public enum UwbSimulationProfile
{
    Static,
    MovingCloser,
    MovingAway,
    PassingBy,
    NoisySignal
}

public interface IUwbProximityProvider : IAblageProximityProvider
{
    UwbProviderStatus Status { get; }

    UwbSimulationProfile Profile { get; }
}

public sealed record UwbSimulationOptions(
    UwbSimulationProfile Profile = UwbSimulationProfile.Static,
    UwbProviderStatus Status = UwbProviderStatus.Simulated);

public sealed class SimulatedUwbProximityProvider : IUwbProximityProvider
{
    private readonly UwbSimulationOptions _options;

    public SimulatedUwbProximityProvider(UwbSimulationOptions? options = null)
    {
        _options = options ?? new UwbSimulationOptions();
    }

    public UwbProviderStatus Status => _options.Status;

    public UwbSimulationProfile Profile => _options.Profile;

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var now = DateTimeOffset.UtcNow;
        return new AblageProximitySnapshot(currentAblageId, CreateSurfaces(currentAblageId, now), now);
    }

    private IReadOnlyList<AblageSurface> CreateSurfaces(AblageIdentity currentAblageId, DateTimeOffset now)
    {
        return _options.Profile switch
        {
            UwbSimulationProfile.MovingCloser => CreateMovingCloser(currentAblageId, now),
            UwbSimulationProfile.MovingAway => CreateMovingAway(currentAblageId, now),
            UwbSimulationProfile.PassingBy => CreatePassingBy(currentAblageId, now),
            UwbSimulationProfile.NoisySignal => CreateNoisySignal(currentAblageId, now),
            _ => CreateStatic(currentAblageId, now)
        };
    }

    private static IReadOnlyList<AblageSurface> CreateStatic(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 1.05, 0.94, now.AddMilliseconds(-80)),
        Surface("ablage-ipad", "Ablage iPad", AblageSurfacePlatform.IOS, AblageDirection.Up, AblageDistanceKind.Medium, 2.20, 0.78, now.AddMilliseconds(-180))
    ];

    private static IReadOnlyList<AblageSurface> CreateMovingCloser(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-ipad", "Ablage iPad", AblageSurfacePlatform.IOS, AblageDirection.Right, AblageDistanceKind.VeryNear, 0.54, 0.97, now.AddMilliseconds(-20)),
        Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.UpRight, AblageDistanceKind.Near, 1.28, 0.89, now.AddMilliseconds(-90))
    ];

    private static IReadOnlyList<AblageSurface> CreateMovingAway(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Medium, 2.75, 0.76, now.AddMilliseconds(-120)),
        Surface("ablage-ipad", "Ablage iPad", AblageSurfacePlatform.IOS, AblageDirection.DownRight, AblageDistanceKind.Far, 3.85, 0.68, now.AddMilliseconds(-210))
    ];

    private static IReadOnlyList<AblageSurface> CreatePassingBy(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-iphone", "Ablage iPhone", AblageSurfacePlatform.IOS, AblageDirection.Up, AblageDistanceKind.Near, 0.92, 0.90, now.AddMilliseconds(-30)),
        Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 1.18, 0.86, now.AddMilliseconds(-70))
    ];

    private static IReadOnlyList<AblageSurface> CreateNoisySignal(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 1.18, 0.71, now.AddMilliseconds(-50)),
        Surface("ablage-ipad", "Ablage iPad", AblageSurfacePlatform.IOS, AblageDirection.UpRight, AblageDistanceKind.Near, 1.24, 0.69, now.AddMilliseconds(-55))
    ];

    private static AblageSurface Current(AblageIdentity currentAblageId, DateTimeOffset now) =>
        new(
            currentAblageId,
            "Ablage Windows",
            AblageSurfacePlatform.Windows,
            true,
            AblagePose.FromDirection(AblageDirection.Unknown),
            AblageDistance.FromSource(AblageDistanceKind.VeryNear, 0.0, 1.0, AblageProximitySource.UWB),
            now);

    private static AblageSurface Surface(
        string id,
        string name,
        AblageSurfacePlatform platform,
        AblageDirection direction,
        AblageDistanceKind distance,
        double meters,
        double confidence,
        DateTimeOffset lastActiveAt)
    {
        return new AblageSurface(
            new AblageIdentity(id),
            name,
            platform,
            true,
            AblagePose.FromDirection(direction),
            AblageDistance.FromSource(distance, meters, confidence, AblageProximitySource.UWB),
            lastActiveAt);
    }
}

public sealed record ProximityFusionSettings
{
    public double ConfidenceThreshold { get; init; } = 0.70;

    public double DistanceHysteresis { get; init; } = 0.24;

    public TimeSpan DirectionStability { get; init; } = TimeSpan.FromMilliseconds(520);

    public IReadOnlyList<AblageProximitySource> ProviderPriority { get; init; } =
    [
        AblageProximitySource.UWB,
        AblageProximitySource.ManualMap,
        AblageProximitySource.WiFi,
        AblageProximitySource.Simulated
    ];
}

public sealed class ProximityFusionProvider : IAblageProximityProvider
{
    private readonly IReadOnlyList<IAblageProximityProvider> _providers;
    private readonly ProximityFusionSettings _settings;

    public ProximityFusionProvider(
        IReadOnlyList<IAblageProximityProvider> providers,
        ProximityFusionSettings? settings = null)
    {
        _providers = providers;
        _settings = settings ?? new ProximityFusionSettings();
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var snapshots = _providers.Select(provider => provider.GetSnapshot(currentAblageId)).ToArray();
        var capturedAt = snapshots.Length == 0 ? DateTimeOffset.UtcNow : snapshots.Max(snapshot => snapshot.CapturedAt);
        var surfaces = snapshots
            .SelectMany(snapshot => snapshot.Surfaces)
            .GroupBy(surface => surface.Id)
            .Select(group => group.Key == currentAblageId ? group.First() : Fuse(group))
            .Where(surface => surface.Distance.Confidence >= _settings.ConfidenceThreshold || surface.Id == currentAblageId)
            .ToArray();

        return new AblageProximitySnapshot(currentAblageId, surfaces, capturedAt);
    }

    private AblageSurface Fuse(IEnumerable<AblageSurface> candidates)
    {
        var ordered = candidates
            .OrderBy(candidate => ProviderRank(candidate.Distance.Source))
            .ThenBy(candidate => candidate.Distance.DistanceMeters ?? candidate.Distance.Rank + 1.0)
            .ThenByDescending(candidate => candidate.Distance.Confidence)
            .ToArray();
        var winner = ordered[0];
        var supportingConfidence = ordered
            .Where(candidate => candidate.Id == winner.Id)
            .Select(candidate => candidate.Distance.Confidence)
            .DefaultIfEmpty(winner.Distance.Confidence)
            .Average();
        var confidence = Math.Clamp(Math.Max(winner.Distance.Confidence, supportingConfidence), 0.0, 1.0);
        var distance = AblageDistance.FromSource(
            winner.Distance.Kind,
            winner.Distance.DistanceMeters,
            confidence,
            AblageProximitySource.SensorFusion);
        return winner with { Distance = distance };
    }

    private int ProviderRank(AblageProximitySource source)
    {
        for (var index = 0; index < _settings.ProviderPriority.Count; index++)
        {
            if (_settings.ProviderPriority[index] == source)
            {
                return index;
            }
        }

        return 99;
    }
}
