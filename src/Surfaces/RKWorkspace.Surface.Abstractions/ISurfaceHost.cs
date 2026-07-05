namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceHost
{
    SurfaceIdentity Identity { get; }

    SurfaceCapabilities Capabilities { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
