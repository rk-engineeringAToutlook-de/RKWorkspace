using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceFramePresenter
{
    Task PresentFrameAsync(FrameSession frameSession, FrameUpdate frameUpdate, CancellationToken cancellationToken);

    Task CloseFrameAsync(string frameSessionId, CancellationToken cancellationToken);
}
