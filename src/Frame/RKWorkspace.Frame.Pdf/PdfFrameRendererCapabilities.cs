namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameRendererCapabilities(
    string RendererName,
    bool SupportsMetadata,
    bool SupportsPageCount,
    bool SupportsFirstPageRendering,
    bool SupportsPngFrame,
    bool Headless,
    bool CrossPlatformCandidate,
    bool OwnerSideOnly,
    bool GuestFileIngressAllowed,
    string LicenseNote,
    string DependencyNote);
