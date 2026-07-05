namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceProximityProvider
{
    Task<IReadOnlyList<SurfaceFramePlacement>> GetNearbyAblagenAsync(CancellationToken cancellationToken);
}
