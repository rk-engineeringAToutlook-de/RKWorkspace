namespace RKWorkspace.Surface.Abstractions;

public sealed record SurfaceGestureEvent(
    GestureType GestureType,
    GestureState State,
    string AblageId,
    double X,
    double Y,
    DateTimeOffset Timestamp);
