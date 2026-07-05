namespace RKWorkspace.Shell;

public interface IAblageProximityProvider
{
    AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId);
}

public interface INearestAblageSelector
{
    NearestAblageResult Select(AblageProximitySnapshot snapshot, NearestAblageResult? previousResult = null);
}

public sealed record NearestAblageSelectionSettings
{
    public double MinimumConfidence { get; init; } = 0.30;

    public double DistanceHysteresis { get; init; } = 0.26;

    public TimeSpan StableNearestDuration { get; init; } = TimeSpan.FromMilliseconds(420);

    public TimeSpan EdgeSwitchDelay { get; init; } = TimeSpan.FromMilliseconds(520);

    public IReadOnlyList<AblageDirection> PreferredDirections { get; init; } =
    [
        AblageDirection.Right,
        AblageDirection.Left,
        AblageDirection.Up,
        AblageDirection.Down
    ];
}

public sealed class NearestAblageSelector : INearestAblageSelector
{
    private readonly NearestAblageSelectionSettings _settings;

    public NearestAblageSelector(NearestAblageSelectionSettings? settings = null)
    {
        _settings = settings ?? new NearestAblageSelectionSettings();
    }

    public NearestAblageResult Select(AblageProximitySnapshot snapshot, NearestAblageResult? previousResult = null)
    {
        var candidates = snapshot.AvailableTargets()
            .Where(surface => surface.Distance.Confidence >= _settings.MinimumConfidence)
            .Select(surface => new Candidate(surface, Score(surface)))
            .OrderBy(candidate => candidate.Score)
            .ThenByDescending(candidate => candidate.Surface.Distance.Confidence)
            .ThenBy(candidate => PreferredDirectionRank(AblageDirectionMapper.ToPrimaryEdge(candidate.Surface.Pose)))
            .ThenByDescending(candidate => candidate.Surface.LastActiveAt)
            .ThenBy(candidate => candidate.Surface.Id.Value, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
        {
            return NearestAblageResult.NoSurfaceAvailable(snapshot.CurrentAblageId);
        }

        var winner = candidates[0];
        var stable = true;
        if (previousResult?.HasTarget == true &&
            previousResult.TargetAblageId != winner.Surface.Id)
        {
            var previousCandidate = candidates.FirstOrDefault(candidate => candidate.Surface.Id == previousResult.TargetAblageId);
            if (previousCandidate is not null &&
                previousCandidate.Score <= winner.Score + _settings.DistanceHysteresis)
            {
                winner = previousCandidate;
                stable = true;
            }
            else
            {
                stable = false;
            }
        }

        return ToResult(snapshot.CurrentAblageId, winner.Surface, stable);
    }

    private double Score(AblageSurface surface)
    {
        var numericDistance = surface.Distance.DistanceMeters ?? (surface.Distance.Rank + 1.0);
        var confidenceBonus = (1.0 - surface.Distance.Confidence) * 0.34;
        var unknownPenalty = surface.Distance.Kind == AblageDistanceKind.Unknown ? 8.0 : 0.0;
        return numericDistance + confidenceBonus + unknownPenalty;
    }

    private int PreferredDirectionRank(AblageDirection direction)
    {
        for (var index = 0; index < _settings.PreferredDirections.Count; index++)
        {
            if (_settings.PreferredDirections[index] == direction)
            {
                return index;
            }
        }

        return 99;
    }

    private static NearestAblageResult ToResult(AblageIdentity currentAblageId, AblageSurface surface, bool stable)
    {
        var edgeHint = AblageDirectionMapper.ToPrimaryEdge(surface.Pose);
        return new NearestAblageResult(
            currentAblageId,
            surface.Id,
            surface.DisplayName,
            surface.Platform,
            surface.Pose.Direction,
            edgeHint,
            surface.Distance.Kind,
            surface.Distance.DistanceMeters,
            surface.Distance.Confidence,
            surface.Distance.Source,
            stable,
            true,
            $"Nearest ablage is {surface.DisplayName} via {edgeHint}.");
    }

    private sealed record Candidate(AblageSurface Surface, double Score);
}

public sealed class SimulatedAblageProximityProvider : IAblageProximityProvider
{
    public static readonly AblageIdentity WindowsAblageId = new("ablage-windows");

    private readonly IReadOnlyList<AblageSurface> _surfaces;

    public SimulatedAblageProximityProvider(IReadOnlyList<AblageSurface>? surfaces = null)
    {
        _surfaces = surfaces ?? CreateDefaultSurfaces();
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        return new AblageProximitySnapshot(currentAblageId, _surfaces, DateTimeOffset.UtcNow);
    }

    public static IReadOnlyList<AblageSurface> CreateDefaultSurfaces()
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            Surface("ablage-windows", "Ablage Windows", AblageSurfacePlatform.Windows, AblageDirection.Unknown, 0, 0, AblageDistanceKind.VeryNear, 0.0, 1.0, now),
            Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, 1, 0, AblageDistanceKind.Near, 1.20, 0.92, now.AddSeconds(-2)),
            Surface("ablage-iphone", "Ablage iPhone", AblageSurfacePlatform.IOS, AblageDirection.Up, 0, -1, AblageDistanceKind.Medium, 2.40, 0.74, now.AddSeconds(-8)),
            Surface("ablage-tablet", "Ablage Tablet", AblageSurfacePlatform.IOS, AblageDirection.DownRight, 0.48, 0.82, AblageDistanceKind.Far, 3.70, 0.68, now.AddSeconds(-14)),
            Surface("ablage-android", "Ablage Android", AblageSurfacePlatform.Android, AblageDirection.Left, -1, 0, AblageDistanceKind.VeryFar, 5.80, 0.58, now.AddSeconds(-21))
        ];
    }

    public static IReadOnlyList<AblageSurface> CreateIPhoneNearestSurfaces()
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            Surface("ablage-windows", "Ablage Windows", AblageSurfacePlatform.Windows, AblageDirection.Unknown, 0, 0, AblageDistanceKind.VeryNear, 0.0, 1.0, now),
            Surface("ablage-macos", "Ablage macOS", AblageSurfacePlatform.MacOS, AblageDirection.Right, 1, 0, AblageDistanceKind.Medium, 2.60, 0.82, now.AddSeconds(-2)),
            Surface("ablage-iphone", "Ablage iPhone", AblageSurfacePlatform.IOS, AblageDirection.Up, 0, -1, AblageDistanceKind.VeryNear, 0.78, 0.90, now.AddSeconds(-1)),
            Surface("ablage-tablet", "Ablage Tablet", AblageSurfacePlatform.IOS, AblageDirection.DownRight, 0.48, 0.82, AblageDistanceKind.Far, 3.70, 0.68, now.AddSeconds(-14)),
            Surface("ablage-android", "Ablage Android", AblageSurfacePlatform.Android, AblageDirection.Left, -1, 0, AblageDistanceKind.VeryFar, 5.80, 0.58, now.AddSeconds(-21))
        ];
    }

    private static AblageSurface Surface(
        string id,
        string name,
        AblageSurfacePlatform platform,
        AblageDirection direction,
        double axisX,
        double axisY,
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
            new AblagePose(direction, axisX, axisY),
            AblageDistance.Simulated(distance, meters, confidence),
            lastActiveAt);
    }
}
