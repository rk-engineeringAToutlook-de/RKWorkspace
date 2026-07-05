using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceInputChannel
{
    Task SendInputAsync(FrameInputEvent inputEvent, CancellationToken cancellationToken);
}
