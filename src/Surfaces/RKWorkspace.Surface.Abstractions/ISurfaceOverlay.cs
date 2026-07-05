namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceOverlay
{
    SurfaceOverlayState State { get; }

    Task ShowGlassEdgeAsync(SurfaceFramePlacement placement, CancellationToken cancellationToken);

    Task HideAsync(CancellationToken cancellationToken);
}
