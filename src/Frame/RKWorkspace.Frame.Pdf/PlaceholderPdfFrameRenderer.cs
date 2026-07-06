namespace RKWorkspace.Frame.Pdf;

public sealed class PlaceholderPdfFrameRenderer : IPdfFrameRenderer
{
    private readonly string _rendererName;
    private DateTimeOffset _lastRenderAt = DateTimeOffset.MinValue;
    private int _renderCount;

    public PlaceholderPdfFrameRenderer(string rendererName = "MetadataPreviewDevRenderer")
    {
        _rendererName = rendererName;
    }

    public PdfFrameRenderResult Render(PdfFrameRenderRequest request)
    {
        request.Validate();
        var document = request.PdfReference;
        _lastRenderAt = DateTimeOffset.UtcNow;
        _renderCount++;

        return new PdfFrameRenderResult(
            request.PageNumber,
            request.Options.RequestedWidth,
            request.Options.RequestedHeight,
            FrameFormat.Placeholder,
            PixelData: null,
            ImagePath: string.Empty,
            Metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["fileName"] = document.FileName,
                ["thingId"] = document.ThingId,
                ["ownerAblageId"] = request.OwnerAblageId,
                ["sha256"] = document.Sha256,
                ["pageCount"] = document.PageCount.ToString(),
                ["lengthBytes"] = document.LengthBytes.ToString(),
                ["noFileIngress"] = "true",
                ["containsOriginalFileBytes"] = "false",
                ["hasOriginalPathForGuest"] = "false"
            },
            _lastRenderAt,
            _rendererName,
            IsPlaceholder: true);
    }

    public PdfFrameRendererDiagnostics GetDiagnostics()
    {
        return new PdfFrameRendererDiagnostics(
            _rendererName,
            SupportsRealRendering: false,
            OwnerSideOnly: true,
            GuestFileIngressAllowed: false,
            Status: "Placeholder metadata renderer; real PDF page rendering is not enabled.",
            _lastRenderAt,
            _renderCount);
    }
}
