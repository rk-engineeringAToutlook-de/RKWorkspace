namespace RKWorkspace.Surface.Abstractions;

public interface ISurfacePlacementAdapter
{
    SurfaceFramePlacement SelectNearestPlacement(IReadOnlyList<SurfaceFramePlacement> placements);
}
