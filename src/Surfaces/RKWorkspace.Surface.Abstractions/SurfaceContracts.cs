namespace RKWorkspace.Surface.Abstractions;

public enum SurfaceOverlayState
{
    Hidden,
    EdgeHint,
    CarryActive,
    FramePresented,
    Returning
}

public sealed record SurfaceIdentity(
    string AblageId,
    SurfacePlatform Platform,
    string DisplayName,
    string ProximityGroup);

public sealed record SurfaceFramePlacement(
    string AblageId,
    string Edge,
    double DistanceMeters,
    bool IsNearest);
