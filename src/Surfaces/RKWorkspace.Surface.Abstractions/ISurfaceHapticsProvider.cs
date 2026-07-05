namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceHapticsProvider
{
    Task HintPickAsync(CancellationToken cancellationToken);

    Task HintPlaceAsync(CancellationToken cancellationToken);
}
