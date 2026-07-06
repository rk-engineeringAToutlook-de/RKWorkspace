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

## Aktueller Dev Renderer

Der aktuelle Renderer heisst:

```text
MetadataPreviewDevRenderer
```

Er liefert:

- echte PDF-Metadaten.
- `FrameFormat: Placeholder`.
- `IsPlaceholder: true`.
- keine Pixel.
- keinen Guest-Pfad.
- keine Originalbytes.
- `NoFileIngress: true`.

Damit ist die Architektur testbar, ohne so zu tun, als waere echtes Seitenrendering bereits erledigt.

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
