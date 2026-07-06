# PDF Frame Renderer Abstraction

Status: MA010.06

## Ziel

PDF Frames duerfen dauerhaft nicht nur Platzhalter bleiben. Die Rendering-Schicht trennt deshalb RKWP Frame/Lease von der konkreten PDF-Rendering-Technik.

Die Gastablage bekommt weiterhin keine freie PDF-Datei. Rendering ist Owner-seitig oder strikt framegebunden.

## Schnittstellen

Implementiert in:

```text
src/Frame/RKWorkspace.Frame.Pdf/
```

Modelle:

- `IPdfFrameRenderer`
- `PdfFrameRenderRequest`
- `PdfFrameRenderResult`
- `PdfFrameRenderOptions`
- `PdfFrameRendererDiagnostics`
- `PdfFrameRendererCapabilities`
- `PdfFrameRendererStatus`
- `PdfFrameRendererException`
- `FrameFormat`

## RenderRequest

Enthaelt:

- `PdfReference`
- `PageNumber`
- `Scale`
- `Rotation`
- `RequestedSize`
- `OwnerAblageId`
- `ThingId`

## RenderResult

Enthaelt:

- `PageNumber`
- `Width`
- `Height`
- `FrameFormat`
- optional `PixelData`
- optional `ImagePath` nur fuer Dev/Diagnose
- `Metadata`
- `RenderedAt`
- `RendererName`
- `IsPlaceholder`

## Aktueller Development Renderer

Der aktuelle echte Development Renderer heisst:

```text
PopplerPdfFrameRenderer
```

Er liefert:

- echte PDF-Metadaten ueber `pdfinfo`.
- echte erste Seite als PNG ueber `pdftoppm`.
- `FrameFormat: PngFrame`.
- `IsPlaceholder: false`.
- PNG-Bytes als Frame-Payload.
- keinen Guest-Pfad.
- keine Originalbytes.
- `NoFileIngress: true`.

Wenn Poppler fehlt, faellt der Default-Pfad auf `MetadataPreviewDevRenderer` zurueck und der Blocker bleibt explizit sichtbar. Das ist ein Development-Fallback, kein Produktziel.

## No File Ingress

Auch ein spaeterer echter Renderer darf nicht:

- Original-PDF speichern.
- Original-PDF an Guest materialisieren.
- Originalpfad an Guest geben.
- eine freie rekonstruierbare PDF-Datei auf Guest erzeugen.

Er darf:

- gerenderte Frames liefern.
- Metadaten liefern.
- Scroll/Zoom/Annotation als erlaubte Frame-Interaktion abbilden.
- temporaeren Cache nur nach FrameCachePolicy nutzen.

## Poppler Development Renderer

`PopplerPdfFrameRenderer` ist owner-seitig, headless und aktuell Development-only. Er sucht Poppler ueber:

- `RKWS_POPPLER_BIN`
- den Codex Runtime Poppler-Pfad
- `PATH`

Er schreibt temporaer nur in ein lokales Temp-Verzeichnis, liest den PNG-Frame sofort in den Speicher und entfernt den temporaeren Ordner danach wieder.
