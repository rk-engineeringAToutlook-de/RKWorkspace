namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameRenderResult(
    int PageNumber,
    int Width,
    int Height,
    FrameFormat FrameFormat,
    byte[]? PixelData,
    string ImagePath,
    IReadOnlyDictionary<string, string> Metadata,
    DateTimeOffset RenderedAt,
    string RendererName,
    bool IsPlaceholder)
{
    public bool ContainsPixelPayload => PixelData is { Length: > 0 };
}
