using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed class PdfFrameOwnerService
{
    private readonly IPdfFrameRenderer _renderer;

    public PdfFrameOwnerService(IPdfFrameRenderer? renderer = null)
    {
        _renderer = renderer ?? new PlaceholderPdfFrameRenderer();
    }

    public PdfFrameSmokeResult OpenFrameOnlySession(
        string pdfPath,
        string ownerAblageId = "ablage-windows-owner",
        string guestAblageId = "ablage-tablet-guest",
        DateTimeOffset? now = null)
    {
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var document = PdfFrameDocument.Load(pdfPath);
        var ownership = ThingOwnership.Original(document.ThingId, ownerAblageId);
        var leasedOwnership = ownership.LeaseToGuest(guestAblageId, OwnershipMode.FrameOnly);
        var lease = CarryLease.Grant(document.ThingId, ownerAblageId, guestAblageId, CarryLeasePolicy.FrameOnlyDefault, timestamp);
        var frame = FrameSession
            .Open(lease, FrameMode.ViewOnly, timestamp)
            .Ready(timestamp.AddMilliseconds(20))
            .Activate(timestamp.AddMilliseconds(40));

        var renderResult = _renderer.Render(new PdfFrameRenderRequest(
            document,
            PageNumber: 1,
            Options: new PdfFrameRenderOptions(),
            ownerAblageId,
            document.ThingId));
        var update = CreateFrameUpdate(document, frame, timestamp.AddMilliseconds(60));
        var guestFrame = new PdfGuestFrame(
            frame.FrameSessionId,
            document.ThingId,
            document.FileName,
            document.PageCount,
            document.Sha256,
            renderResult.IsPlaceholder ? PdfFrameRepresentationKind.MetadataPreview : PdfFrameRepresentationKind.RenderedFirstPage,
            DisplayText: $"FrameOnly preview of {document.FileName}; pages={document.PageCount}; sha256={document.Sha256[..16]}",
            RendererStatus: renderResult.IsPlaceholder ? "RendererBlocked" : "Rendered",
            RendererName: renderResult.RendererName,
            IsPlaceholder: renderResult.IsPlaceholder,
            FrameFormat: renderResult.FrameFormat,
            SupportsScroll: true,
            SupportsZoom: true,
            ContainsOriginalFileBytes: false,
            HasOriginalFilePath: false);

        var returnedLease = lease.Return(timestamp.AddSeconds(1));
        var recovered = lease with { LastHeartbeat = timestamp.AddSeconds(-30) };
        var recovery = recovered.Advance(timestamp).Recover();

        return new PdfFrameSmokeResult(
            document,
            leasedOwnership,
            lease,
            frame,
            update,
            guestFrame,
            returnedLease,
            recovery);
    }

    private static FrameUpdate CreateFrameUpdate(PdfFrameDocument document, FrameSession frame, DateTimeOffset timestamp)
    {
        return new FrameUpdate(
            frame.FrameSessionId,
            document.ThingId,
            PageNumber: 1,
            Representation: $"PDF frame representation; name={document.FileName}; pages={document.PageCount}; sha256={document.Sha256[..16]}",
            ContentHash: document.Sha256,
            ContainsOriginalFileBytes: false,
            Timestamp: timestamp);
    }
}

public sealed record PdfGuestFrame(
    string FrameSessionId,
    string ThingId,
    string DisplayName,
    int PageCount,
    string SourceHash,
    PdfFrameRepresentationKind RepresentationKind,
    string DisplayText,
    string RendererStatus,
    string RendererName,
    bool IsPlaceholder,
    FrameFormat FrameFormat,
    bool SupportsScroll,
    bool SupportsZoom,
    bool ContainsOriginalFileBytes,
    bool HasOriginalFilePath);

public enum PdfFrameRepresentationKind
{
    MetadataPreview,
    RenderedFirstPage,
    RenderedPageImage
}
