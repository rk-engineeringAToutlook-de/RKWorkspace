namespace RKWorkspace.Shell;

public interface IDongleAnchorProvider : IAblageProximityProvider
{
    string DongleId { get; }

    UwbProviderStatus Status { get; }

    UwbSimulationProfile Profile { get; }

    bool SupportsBlePresence { get; }

    bool SupportsUwbRanging { get; }

    string PrivacyMode { get; }

    IReadOnlyList<DongleAnchorReading> GetAnchorReadings(AblageIdentity currentAblageId);
}

public sealed record DongleAnchorSimulationOptions(
    string DongleId = "dongle-lab-anchor-01",
    UwbSimulationProfile Profile = UwbSimulationProfile.Static,
    UwbProviderStatus Status = UwbProviderStatus.Simulated,
    bool SupportsBlePresence = true,
    bool SupportsUwbRanging = true,
    string PrivacyMode = "EphemeralLab");

public sealed record DongleAnchorReading(
    string DongleId,
    string EphemeralBeaconId,
    AblageSurface Surface,
    bool BlePresent,
    bool UwbRangePresent,
    UwbProviderStatus Status);

public sealed class SimulatedDongleAnchorProvider : IDongleAnchorProvider
{
    private readonly DongleAnchorSimulationOptions _options;

    public SimulatedDongleAnchorProvider(DongleAnchorSimulationOptions? options = null)
    {
        _options = options ?? new DongleAnchorSimulationOptions();
    }

    public string DongleId => _options.DongleId;

    public UwbProviderStatus Status => _options.Status;

    public UwbSimulationProfile Profile => _options.Profile;

    public bool SupportsBlePresence => _options.SupportsBlePresence;

    public bool SupportsUwbRanging => _options.SupportsUwbRanging;

    public string PrivacyMode => _options.PrivacyMode;

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var readings = GetAnchorReadings(currentAblageId);
        var capturedAt = readings.Count == 0
            ? DateTimeOffset.UtcNow
            : readings.Max(reading => reading.Surface.LastActiveAt);
        return new AblageProximitySnapshot(currentAblageId, readings.Select(reading => reading.Surface).ToArray(), capturedAt);
    }

    public IReadOnlyList<DongleAnchorReading> GetAnchorReadings(AblageIdentity currentAblageId)
    {
        var now = DateTimeOffset.UtcNow;
        var surfaces = _options.Profile switch
        {
            UwbSimulationProfile.MovingCloser => CreateMovingCloser(currentAblageId, now),
            UwbSimulationProfile.MovingAway => CreateMovingAway(currentAblageId, now),
            UwbSimulationProfile.PassingBy => CreatePassingBy(currentAblageId, now),
            UwbSimulationProfile.NoisySignal => CreateNoisySignal(currentAblageId, now),
            _ => CreateStatic(currentAblageId, now)
        };

        return surfaces
            .Select((surface, index) => new DongleAnchorReading(
                _options.DongleId,
                CreateEphemeralBeaconId(surface.Id, index),
                surface,
                _options.SupportsBlePresence,
                _options.SupportsUwbRanging,
                _options.Status))
            .ToArray();
    }

    private string CreateEphemeralBeaconId(AblageIdentity ablageId, int index)
    {
        return _options.PrivacyMode.Equals("EphemeralLab", StringComparison.OrdinalIgnoreCase)
            ? $"{_options.DongleId}-ephemeral-{index + 1}-{ablageId.Value.GetHashCode():x8}"
            : $"{_options.DongleId}-{ablageId.Value}";
    }

    private IReadOnlyList<AblageSurface> CreateStatic(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-dongle-macos", "Ablage macOS via Dongle", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 0.96, 0.93, now.AddMilliseconds(-42)),
        Surface("ablage-dongle-ipad", "Ablage iPad via Dongle", AblageSurfacePlatform.IOS, AblageDirection.Up, AblageDistanceKind.Medium, 1.90, 0.81, now.AddMilliseconds(-88))
    ];

    private IReadOnlyList<AblageSurface> CreateMovingCloser(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-dongle-ipad", "Ablage iPad via Dongle", AblageSurfacePlatform.IOS, AblageDirection.Right, AblageDistanceKind.VeryNear, 0.48, 0.96, now.AddMilliseconds(-18)),
        Surface("ablage-dongle-macos", "Ablage macOS via Dongle", AblageSurfacePlatform.MacOS, AblageDirection.UpRight, AblageDistanceKind.Near, 1.12, 0.86, now.AddMilliseconds(-64))
    ];

    private IReadOnlyList<AblageSurface> CreateMovingAway(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-dongle-macos", "Ablage macOS via Dongle", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Medium, 2.48, 0.74, now.AddMilliseconds(-96)),
        Surface("ablage-dongle-ipad", "Ablage iPad via Dongle", AblageSurfacePlatform.IOS, AblageDirection.DownRight, AblageDistanceKind.Far, 3.42, 0.66, now.AddMilliseconds(-144))
    ];

    private IReadOnlyList<AblageSurface> CreatePassingBy(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-dongle-iphone", "Ablage iPhone via Dongle", AblageSurfacePlatform.IOS, AblageDirection.Up, AblageDistanceKind.Near, 0.86, 0.90, now.AddMilliseconds(-26)),
        Surface("ablage-dongle-macos", "Ablage macOS via Dongle", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 1.02, 0.84, now.AddMilliseconds(-54))
    ];

    private IReadOnlyList<AblageSurface> CreateNoisySignal(AblageIdentity currentAblageId, DateTimeOffset now) =>
    [
        Current(currentAblageId, now),
        Surface("ablage-dongle-macos", "Ablage macOS via Dongle", AblageSurfacePlatform.MacOS, AblageDirection.Right, AblageDistanceKind.Near, 1.08, 0.72, now.AddMilliseconds(-48)),
        Surface("ablage-dongle-ipad", "Ablage iPad via Dongle", AblageSurfacePlatform.IOS, AblageDirection.UpRight, AblageDistanceKind.Near, 1.16, 0.70, now.AddMilliseconds(-52))
    ];

    private static AblageSurface Current(AblageIdentity currentAblageId, DateTimeOffset now) =>
        new(
            currentAblageId,
            "Ablage Windows",
            AblageSurfacePlatform.Windows,
            true,
            AblagePose.FromDirection(AblageDirection.Unknown),
            AblageDistance.FromSource(AblageDistanceKind.VeryNear, 0.0, 1.0, AblageProximitySource.Dongle),
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
            AblageDistance.FromSource(distance, meters, confidence, AblageProximitySource.Dongle),
            lastActiveAt);
    }
}
