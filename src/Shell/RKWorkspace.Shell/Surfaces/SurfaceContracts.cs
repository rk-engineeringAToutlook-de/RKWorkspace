namespace RKWorkspace.Shell;

public interface ISurfaceHost
{
    AblageIdentity AblageId { get; }

    AblageSurfacePlatform Platform { get; }
}

public interface ISurfaceOverlay
{
    void ShowGlassEdge(GlassEdge edge);

    void HideGlassEdge();
}

public interface ISurfaceGestureProvider
{
    bool IsPickGestureAvailable { get; }

    bool IsPlaceGestureAvailable { get; }
}

public interface ISurfaceHapticsProvider
{
    bool IsAvailable { get; }

    void TryPulse(SurfaceHapticMoment moment);
}

public interface ISurfaceProximityProvider : IAblageProximityProvider
{
}

public interface ISurfaceEdgeRenderer
{
    bool SupportsDirection(AblageDirection direction);

    bool SupportsTransparentGlass { get; }
}

public interface ISurfaceObjectCaptureAdapter
{
    bool CanCaptureCurrentObject { get; }
}

public interface ISurfacePlacementAdapter
{
    WorkspaceSurfacePlacement PreparePlacement(IncomingSurfaceObject incomingObject);
}

public enum SurfaceHapticMoment
{
    GestureRecognized,
    ObjectPicked,
    EdgeAppeared,
    EdgeActive,
    ObjectEntered,
    ObjectPlaced
}
