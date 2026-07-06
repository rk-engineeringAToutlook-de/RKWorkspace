using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed class PdfFrameOwnerService
{
    private readonly IPdfFrameRenderer _renderer;
    private readonly FrameCachePolicy _cachePolicy;

    public PdfFrameOwnerService(IPdfFrameRenderer? renderer = null, FrameCachePolicy? cachePolicy = null)
    {
        _renderer = renderer ?? PdfFrameRendererFactory.CreateDefault();
        _cachePolicy = cachePolicy ?? FrameCachePolicy.DevelopmentMemoryOnly;
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
        var frameCache = new FrameCache(_cachePolicy);
        var documentFrameState = PdfDocumentFrameState.Open(document, frame, timestamp.AddMilliseconds(60));
        if (document.PageCount > 1)
        {
            documentFrameState = documentFrameState.NextPage(document, frame, timestamp.AddMilliseconds(65));
        }

        documentFrameState = documentFrameState.PreviousPage(document, frame, timestamp.AddMilliseconds(68));
        var update = documentFrameState.Updates[0].FrameUpdate;
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
        frameCache.Store(frame.FrameSessionId, document.ThingId, renderResult, timestamp.AddMilliseconds(70));
        var cacheBeforeClose = frameCache.GetDiagnostics();

        var returnedLease = lease.Return(timestamp.AddSeconds(1));
        frameCache.EvictFrameSession(frame.FrameSessionId, FrameCacheEvictionReason.FrameClose);
        var cacheAfterClose = frameCache.GetDiagnostics();
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
            recovery,
            documentFrameState,
            _cachePolicy,
            cacheBeforeClose,
            cacheAfterClose);
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
