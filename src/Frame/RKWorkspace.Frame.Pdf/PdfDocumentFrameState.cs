using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed record PdfPageReference(
    string ThingId,
    int PageNumber,
    int PageCount,
    string ContentHash)
{
    public bool IsFirstPage => PageNumber == 1;

    public bool IsLastPage => PageNumber == PageCount;

    public static PdfPageReference FromDocument(PdfFrameDocument document, int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > document.PageCount)
        {
            throw new PdfFrameException("PDF page is outside the document frame range.");
        }

        return new PdfPageReference(document.ThingId, pageNumber, document.PageCount, document.Sha256);
    }
}

public sealed record PdfPageFrame(
    PdfPageReference Page,
    PdfFrameRenderResult RenderResult)
{
    public bool ContainsOriginalFileBytes => false;

    public bool NoFileIngress =>
        !ContainsOriginalFileBytes &&
        RenderResult.Metadata.TryGetValue("containsOriginalFileBytes", out var containsOriginal) &&
        string.Equals(containsOriginal, bool.FalseString, StringComparison.OrdinalIgnoreCase);
}

public sealed record PageFrameUpdate(
    FrameUpdate FrameUpdate,
    PdfPageReference Page,
    bool IsTileUpdate = false);

public sealed record PdfDocumentFrameState(
    string FrameSessionId,
    string ThingId,
    int PageCount,
    int CurrentPage,
    PdfPageReference CurrentReference,
    IReadOnlyList<PageFrameUpdate> Updates)
{
    public static PdfDocumentFrameState Open(PdfFrameDocument document, FrameSession frameSession, DateTimeOffset now)
    {
        var reference = PdfPageReference.FromDocument(document, 1);
        return new PdfDocumentFrameState(
            frameSession.FrameSessionId,
            document.ThingId,
            document.PageCount,
            CurrentPage: 1,
            reference,
            [CreateUpdate(document, frameSession, reference, now)]);
    }

    public PdfDocumentFrameState NextPage(PdfFrameDocument document, FrameSession frameSession, DateTimeOffset now)
    {
        var nextPage = Math.Min(PageCount, CurrentPage + 1);
        return MoveToPage(document, frameSession, nextPage, now);
    }

    public PdfDocumentFrameState PreviousPage(PdfFrameDocument document, FrameSession frameSession, DateTimeOffset now)
    {
        var previousPage = Math.Max(1, CurrentPage - 1);
        return MoveToPage(document, frameSession, previousPage, now);
    }

    public PdfDocumentFrameState MoveToPage(PdfFrameDocument document, FrameSession frameSession, int pageNumber, DateTimeOffset now)
    {
        var reference = PdfPageReference.FromDocument(document, pageNumber);
        var update = CreateUpdate(document, frameSession, reference, now);
        return this with
        {
            CurrentPage = pageNumber,
            CurrentReference = reference,
            Updates = Updates.Concat([update]).ToArray()
        };
    }

    private static PageFrameUpdate CreateUpdate(
        PdfFrameDocument document,
        FrameSession frameSession,
        PdfPageReference reference,
        DateTimeOffset now)
    {
        var frameUpdate = new FrameUpdate(
            frameSession.FrameSessionId,
            document.ThingId,
            reference.PageNumber,
            $"PDF page frame; name={document.FileName}; page={reference.PageNumber}/{reference.PageCount}; sha256={document.Sha256[..16]}",
            reference.ContentHash,
            ContainsOriginalFileBytes: false,
            Timestamp: now);

        return new PageFrameUpdate(frameUpdate, reference);
    }
}
