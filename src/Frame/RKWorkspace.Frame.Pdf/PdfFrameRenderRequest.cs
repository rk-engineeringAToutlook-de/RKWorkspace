namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameRenderRequest(
    PdfFrameDocument PdfReference,
    int PageNumber,
    PdfFrameRenderOptions Options,
    string OwnerAblageId,
    string ThingId)
{
    public PdfFrameRenderRequest Validate()
    {
        if (PageNumber <= 0)
        {
            throw new PdfFrameRendererException("PageNumber must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(OwnerAblageId))
        {
            throw new PdfFrameRendererException("OwnerAblageId is required.");
        }

        if (string.IsNullOrWhiteSpace(ThingId))
        {
            throw new PdfFrameRendererException("ThingId is required.");
        }

        return this;
    }
}
