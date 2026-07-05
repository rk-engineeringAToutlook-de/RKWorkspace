namespace RKWorkspace.Shell;

public sealed record ManualAblageMap(
    AblageIdentity AblageId,
    string DisplayName,
    AblageDirection RelativeDirection,
    AblageDistanceKind DistanceClass,
    double? DistanceMeters,
    double Confidence,
    DateTimeOffset LastUpdated,
    AblageProximitySource Source,
    AblageSurfacePlatform Platform = AblageSurfacePlatform.Unknown,
    bool IsAvailable = true)
{
    public AblageSurface ToSurface()
    {
        return new AblageSurface(
            AblageId,
            DisplayName,
            Platform,
            IsAvailable,
            AblagePose.FromDirection(RelativeDirection),
            AblageDistance.FromSource(DistanceClass, DistanceMeters, Confidence, Source),
            LastUpdated);
    }
}

public sealed class ManualMapAblageProximityProvider : IAblageProximityProvider
{
    private readonly IReadOnlyList<ManualAblageMap> _map;

    public ManualMapAblageProximityProvider(IReadOnlyList<ManualAblageMap> map)
    {
        _map = map;
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var surfaces = _map.Select(entry => entry.ToSurface()).ToArray();
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

    public static IReadOnlyList<ManualAblageMap> CreateOwnerRoomExample(DateTimeOffset? capturedAt = null)
    {
        var now = capturedAt ?? DateTimeOffset.UtcNow;
        return
        [
            new ManualAblageMap(
                SimulatedAblageProximityProvider.WindowsAblageId,
                "Ablage Windows",
                AblageDirection.Unknown,
                AblageDistanceKind.VeryNear,
                0,
                1.0,
                now,
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.Windows),
            new ManualAblageMap(
                new AblageIdentity("ablage-macos"),
                "Ablage macOS",
                AblageDirection.Right,
                AblageDistanceKind.Near,
                1.10,
                0.93,
                now.AddSeconds(-1),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.MacOS),
            new ManualAblageMap(
                new AblageIdentity("ablage-ipad"),
                "Ablage iPad",
                AblageDirection.Up,
                AblageDistanceKind.Medium,
                2.35,
                0.82,
                now.AddSeconds(-2),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.IOS),
            new ManualAblageMap(
                new AblageIdentity("ablage-iphone"),
                "Ablage iPhone",
                AblageDirection.Down,
                AblageDistanceKind.Near,
                1.45,
                0.78,
                now.AddSeconds(-3),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.IOS),
            new ManualAblageMap(
                new AblageIdentity("ablage-monitor-links"),
                "Ablage Monitor links",
                AblageDirection.Left,
                AblageDistanceKind.Far,
                4.80,
                0.64,
                now.AddSeconds(-4),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.Unknown)
        ];
    }
}
