using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Surface.Abstractions;

public enum WorkspaceSurfacePlatform
{
    Windows,
    MacOS,
    IOS,
    IPadOS,
    Android,
    Linux,
    Unknown
}

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
    WorkspaceSurfacePlatform Platform,
    string DisplayName,
    string ProximityGroup);

public sealed record SurfaceFramePlacement(
    string AblageId,
    string Edge,
    double DistanceMeters,
    bool IsNearest);

public interface ISurfaceHost
{
    SurfaceIdentity Identity { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}

public interface ISurfaceOverlay
{
    SurfaceOverlayState State { get; }

    Task ShowGlassEdgeAsync(SurfaceFramePlacement placement, CancellationToken cancellationToken);

    Task HideAsync(CancellationToken cancellationToken);
}

public interface ISurfaceFramePresenter
{
    Task PresentFrameAsync(FrameSession frameSession, FrameUpdate frameUpdate, CancellationToken cancellationToken);

    Task CloseFrameAsync(string frameSessionId, CancellationToken cancellationToken);
}

public interface ISurfaceInputChannel
{
    Task SendInputAsync(FrameInputEvent inputEvent, CancellationToken cancellationToken);
}

public interface ISurfacePlacementAdapter
{
    SurfaceFramePlacement SelectNearestPlacement(IReadOnlyList<SurfaceFramePlacement> placements);
}

public interface ISurfaceObjectAdapter
{
    bool CanRepresentAsWorkspaceObject(string nativeObjectKind);

    string CreateThingId(string nativeObjectId);
}

public interface ISurfaceSecurityContext
{
    bool RequiresSecureSession { get; }

    bool AllowsFrameOnlyPresentation { get; }

    bool AllowsOriginalFileIngress { get; }
}
