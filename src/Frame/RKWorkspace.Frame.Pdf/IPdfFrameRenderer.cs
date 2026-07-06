namespace RKWorkspace.Frame.Pdf;

public interface IPdfFrameRenderer
{
    PdfFrameRenderResult Render(PdfFrameRenderRequest request);

    PdfFrameRendererDiagnostics GetDiagnostics();
}
