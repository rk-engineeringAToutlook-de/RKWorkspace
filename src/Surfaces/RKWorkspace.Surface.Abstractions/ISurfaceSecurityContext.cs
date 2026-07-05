namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceSecurityContext
{
    bool RequiresSecureSession { get; }

    bool AllowsFrameOnlyPresentation { get; }

    bool AllowsOriginalFileIngress { get; }
}
