namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameRendererDiagnostics(
    string RendererName,
    bool SupportsRealRendering,
    bool OwnerSideOnly,
    bool GuestFileIngressAllowed,
    string Status,
    DateTimeOffset LastRenderAt,
    int RenderCount);
