using RKWorkspace.Protocol.Ownership;
using System.Globalization;

namespace RKWorkspace.Frame.Pdf;

public sealed record DirtyRegion(
    double X,
    double Y,
    double Width,
    double Height)
{
    public bool IsValid => Width > 0 && Height > 0;
}

public sealed record FrameTile(
    int PageNumber,
    int TileColumn,
    int TileRow,
    int Width,
    int Height,
    double Scale,
    DirtyRegion DirtyRegion,
    string ContentHash)
{
    public string TileId => $"page-{PageNumber}-tile-{TileColumn}-{TileRow}-scale-{Scale:0.###}";
}

public sealed record PdfViewportState(
    int PageNumber,
    double Zoom,
    double ScrollX,
    double ScrollY,
    int ViewportWidth,
    int ViewportHeight)
{
    public PdfViewportState ChangeZoom(double zoom)
    {
        if (zoom <= 0)
        {
            throw new PdfFrameException("PDF viewport zoom must be greater than zero.");
        }

        return this with { Zoom = zoom };
    }

    public PdfViewportState ScrollTo(double scrollX, double scrollY)
    {
        return this with
        {
            ScrollX = Math.Max(0, scrollX),
            ScrollY = Math.Max(0, scrollY)
        };
    }
}

public sealed record PdfTileRequest(
    string FrameSessionId,
    string ThingId,
    PdfViewportState Viewport,
    int TileColumn,
    int TileRow)
{
    public PdfTileRequest Validate(PdfFrameDocument document)
    {
        if (Viewport.PageNumber < 1 || Viewport.PageNumber > document.PageCount)
        {
            throw new PdfFrameException("PDF tile page is outside the document frame range.");
        }

        if (TileColumn < 0 || TileRow < 0)
        {
            throw new PdfFrameException("PDF tile coordinates must not be negative.");
        }

        return this;
    }
}

public sealed record PdfTileResponse(
    PdfTileRequest Request,
    FrameTile Tile,
    PageFrameUpdate Update,
    bool ContainsOriginalFileBytes)
{
    public bool NoFileIngress => !ContainsOriginalFileBytes && !Update.FrameUpdate.ContainsOriginalFileBytes;
}

public static class PdfTilePipeline
{
    public static PdfTileResponse CreateTile(
        PdfFrameDocument document,
        FrameSession frameSession,
        PdfTileRequest request,
        DateTimeOffset now)
    {
        request.Validate(document);
        var tileWidth = Math.Max(1, request.Viewport.ViewportWidth / 2);
        var tileHeight = Math.Max(1, request.Viewport.ViewportHeight / 2);
        var dirtyRegion = new DirtyRegion(
            request.TileColumn * tileWidth,
            request.TileRow * tileHeight,
            tileWidth,
            tileHeight);
        var tile = new FrameTile(
            request.Viewport.PageNumber,
            request.TileColumn,
            request.TileRow,
            tileWidth,
            tileHeight,
            request.Viewport.Zoom,
            dirtyRegion,
            document.Sha256);
        var dirtyText = string.Create(
            CultureInfo.InvariantCulture,
            $"{dirtyRegion.X:0},{dirtyRegion.Y:0},{dirtyRegion.Width:0},{dirtyRegion.Height:0}");
        var zoomText = request.Viewport.Zoom.ToString("0.###", CultureInfo.InvariantCulture);
        var page = PdfPageReference.FromDocument(document, request.Viewport.PageNumber);
        var frameUpdate = new FrameUpdate(
            frameSession.FrameSessionId,
            document.ThingId,
            request.Viewport.PageNumber,
            $"PDF tile update; {tile.TileId}; dirty={dirtyText}; zoom={zoomText}",
            document.Sha256,
            ContainsOriginalFileBytes: false,
            Timestamp: now);

        return new PdfTileResponse(
            request,
            tile,
            new PageFrameUpdate(frameUpdate, page, IsTileUpdate: true),
            ContainsOriginalFileBytes: false);
    }
}
