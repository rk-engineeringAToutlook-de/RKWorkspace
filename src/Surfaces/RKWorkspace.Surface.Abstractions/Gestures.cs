namespace RKWorkspace.Surface.Abstractions;

public enum SurfaceGestureType
{
    ThreeFingerHold,
    LongPress,
    MouseLongPress,
    KeyboardActivation,
    TouchHold,
    PenHold,
    Unknown
}

public enum SurfaceGestureState
{
    Started,
    Recognized,
    Cancelled,
    Completed,
    Failed
}

public sealed record SurfaceGestureEvent(
    SurfaceGestureType GestureType,
    SurfaceGestureState State,
    string AblageId,
    double X,
    double Y,
    DateTimeOffset Timestamp);

public interface ISurfaceGestureProvider
{
    IAsyncEnumerable<SurfaceGestureEvent> ReadGesturesAsync(CancellationToken cancellationToken);
}

public interface ISurfaceHapticsProvider
{
    Task HintPickAsync(CancellationToken cancellationToken);

    Task HintPlaceAsync(CancellationToken cancellationToken);
}

public interface ISurfaceProximityProvider
{
    Task<IReadOnlyList<SurfaceFramePlacement>> GetNearbyAblagenAsync(CancellationToken cancellationToken);
}
