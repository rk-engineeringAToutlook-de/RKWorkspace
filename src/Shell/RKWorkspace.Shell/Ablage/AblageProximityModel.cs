namespace RKWorkspace.Shell;

public readonly record struct AblageIdentity(string Value)
{
    public override string ToString() => Value;
}

public enum AblageSurfacePlatform
{
    Unknown,
    Windows,
    MacOS,
    IOS,
    Android
}

public enum AblageDirection
{
    Unknown,
    Left,
    Right,
    Up,
    Down,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight,
    Front,
    Back
}

public enum AblageDistanceKind
{
    Unknown,
    VeryNear,
    Near,
    Medium,
    Far,
    VeryFar
}

public enum AblageDistanceSource
{
    Unknown,
    Simulated,
    ManualMap,
    BLE,
    UWB,
    Dongle,
    WiFi,
    SensorFusion
}

public sealed record AblageDistance(
    AblageDistanceKind Kind,
    double? DistanceMeters,
    double Confidence,
    AblageDistanceSource Source)
{
    public static AblageDistance Simulated(AblageDistanceKind kind, double meters, double confidence)
    {
        return new AblageDistance(kind, meters, Math.Clamp(confidence, 0.0, 1.0), AblageDistanceSource.Simulated);
    }

    public double Rank => Kind switch
    {
        AblageDistanceKind.VeryNear => 0,
        AblageDistanceKind.Near => 1,
        AblageDistanceKind.Medium => 2,
        AblageDistanceKind.Far => 3,
        AblageDistanceKind.VeryFar => 4,
        _ => 9
    };
}

public sealed record AblagePose(
    AblageDirection Direction,
    double AxisX,
    double AxisY)
{
    public static AblagePose FromDirection(AblageDirection direction)
    {
        return direction switch
        {
            AblageDirection.Left => new AblagePose(direction, -1, 0),
            AblageDirection.Right => new AblagePose(direction, 1, 0),
            AblageDirection.Up => new AblagePose(direction, 0, -1),
            AblageDirection.Down => new AblagePose(direction, 0, 1),
            AblageDirection.UpLeft => new AblagePose(direction, -0.72, -0.72),
            AblageDirection.UpRight => new AblagePose(direction, 0.72, -0.72),
            AblageDirection.DownLeft => new AblagePose(direction, -0.72, 0.72),
            AblageDirection.DownRight => new AblagePose(direction, 0.72, 0.72),
            AblageDirection.Front => new AblagePose(direction, 0, -0.25),
            AblageDirection.Back => new AblagePose(direction, 0, 0.25),
            _ => new AblagePose(AblageDirection.Unknown, 0, 0)
        };
    }
}

public sealed record AblageSurface(
    AblageIdentity Id,
    string DisplayName,
    AblageSurfacePlatform Platform,
    bool IsAvailable,
    AblagePose Pose,
    AblageDistance Distance,
    DateTimeOffset LastActiveAt);

public sealed record AblageProximitySnapshot(
    AblageIdentity CurrentAblageId,
    IReadOnlyList<AblageSurface> Surfaces,
    DateTimeOffset CapturedAt)
{
    public IEnumerable<AblageSurface> AvailableTargets()
    {
        return Surfaces.Where(surface => surface.IsAvailable && surface.Id != CurrentAblageId);
    }
}

public sealed record NearestAblageResult(
    AblageIdentity CurrentAblageId,
    AblageIdentity? TargetAblageId,
    string TargetDisplayName,
    AblageSurfacePlatform Platform,
    AblageDirection Direction,
    AblageDirection EdgeHint,
    AblageDistanceKind Distance,
    double? DistanceMeters,
    double Confidence,
    AblageDistanceSource Source,
    bool IsStable,
    bool HasTarget,
    string Reason)
{
    public static NearestAblageResult NoSurfaceAvailable(AblageIdentity currentAblageId)
    {
        return new NearestAblageResult(
            currentAblageId,
            null,
            string.Empty,
            AblageSurfacePlatform.Unknown,
            AblageDirection.Unknown,
            AblageDirection.Unknown,
            AblageDistanceKind.Unknown,
            null,
            0,
            AblageDistanceSource.Unknown,
            true,
            false,
            "No available ablage surface.");
    }
}

public static class AblageDirectionMapper
{
    public static AblageDirection ToPrimaryEdge(AblagePose pose)
    {
        return pose.Direction switch
        {
            AblageDirection.Left or AblageDirection.Right or AblageDirection.Up or AblageDirection.Down => pose.Direction,
            AblageDirection.UpLeft or AblageDirection.UpRight or AblageDirection.DownLeft or AblageDirection.DownRight =>
                Math.Abs(pose.AxisX) >= Math.Abs(pose.AxisY)
                    ? (pose.AxisX >= 0 ? AblageDirection.Right : AblageDirection.Left)
                    : (pose.AxisY >= 0 ? AblageDirection.Down : AblageDirection.Up),
            AblageDirection.Front => AblageDirection.Up,
            AblageDirection.Back => AblageDirection.Down,
            _ => AblageDirection.Unknown
        };
    }

    public static AblageDirection Opposite(AblageDirection direction)
    {
        return direction switch
        {
            AblageDirection.Left => AblageDirection.Right,
            AblageDirection.Right => AblageDirection.Left,
            AblageDirection.Up => AblageDirection.Down,
            AblageDirection.Down => AblageDirection.Up,
            AblageDirection.UpLeft => AblageDirection.DownRight,
            AblageDirection.UpRight => AblageDirection.DownLeft,
            AblageDirection.DownLeft => AblageDirection.UpRight,
            AblageDirection.DownRight => AblageDirection.UpLeft,
            AblageDirection.Front => AblageDirection.Back,
            AblageDirection.Back => AblageDirection.Front,
            _ => AblageDirection.Unknown
        };
    }
}
