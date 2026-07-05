namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceGestureProvider
{
    IAsyncEnumerable<SurfaceGestureEvent> ReadGesturesAsync(CancellationToken cancellationToken);
}
