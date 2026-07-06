namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceHapticsProvider
{
    HapticCapability Capability { get; }

    Task PlayAsync(HapticHint hint, CancellationToken cancellationToken);

    Task HintPickAsync(CancellationToken cancellationToken);

    Task HintPlaceAsync(CancellationToken cancellationToken);
}
