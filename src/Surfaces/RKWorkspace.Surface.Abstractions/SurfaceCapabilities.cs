namespace RKWorkspace.Surface.Abstractions;

[Flags]
public enum SurfaceCapabilities
{
    None = 0,
    Overlay = 1 << 0,
    GlassEdge = 1 << 1,
    FramePresentation = 1 << 2,
    GestureInput = 1 << 3,
    Haptics = 1 << 4,
    Proximity = 1 << 5,
    SecureContext = 1 << 6,
    FilePicker = 1 << 7,
    ShareExtension = 1 << 8,
    Clipboard = 1 << 9
}
