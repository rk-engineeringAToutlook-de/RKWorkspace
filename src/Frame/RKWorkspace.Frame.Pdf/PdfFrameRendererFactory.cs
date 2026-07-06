namespace RKWorkspace.Frame.Pdf;

public static class PdfFrameRendererFactory
{
    public static IPdfFrameRenderer CreateDefault()
    {
        return PopplerPdfFrameRenderer.TryCreate(out var renderer, out _)
            ? renderer
            : new PlaceholderPdfFrameRenderer();
    }
}
