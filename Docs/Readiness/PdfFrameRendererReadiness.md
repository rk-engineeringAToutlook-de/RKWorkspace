# PDF Frame Renderer Readiness

Status: MA013.40  
Datum: 2026-07-06

## Ziel

MA013.40 schliesst den PDF/Frame-Status fuer den naechsten Pilotabschnitt ab.

## Bewertung

| Bereich | Status | Bemerkung |
| --- | --- | --- |
| Renderer | Pilotbereit | Poppler Dev-Renderer aktiv, PDFium als Produktkandidat |
| Placeholder | Stabil | expliziter RendererBlocked-Pfad bleibt erlaubt |
| Multipage | Vorbereitet | `PdfDocumentFrameState`, `PdfPageReference`, `PdfPageFrame`, `PageFrameUpdate` |
| Zoom | Vorbereitet | `PdfViewportState` und Tile-Pipeline |
| Tiles | Vorbereitet | `FrameTile`, `DirtyRegion`, `PdfTileRequest`, `PdfTileResponse` |
| Annotation | Vorbereitet | Highlight, Note, Rectangle, FreeText planned |
| Extraction | Policy-Gate | default denied, allowed by policy, audit |
| Security | Dokumentiert | Renderer Sandbox Model, Malformed PDF Handling |
| Performance | Messbar | First/Next page, tile, frame size, memory |

## Pilotfaehig

Ja, fuer Development- und Owner-Lab-Tests.

## Nicht produktfreigegeben

Noch offen vor Produkt:

- PDFium/MuPDF/commercial Lizenzentscheidung finalisieren.
- Renderer-Prozesssandbox implementieren.
- Malformed-PDF-Fuzzing erweitern.
- macOS/iPad Guest real anschliessen.
- Produkttransport statt DevTransport.
- Mobile FramePresenter visuell validieren.

## Pflichttests

```powershell
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-rkwp-tests.ps1
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-tests.ps1
```

## Ergebnis

Der PDF Frame Renderer ist fuer den naechsten Pilotblock ausreichend vorbereitet. Produktreife beginnt erst nach Renderer-Sandbox, echter Cross-Device-Strecke und finaler Lizenzentscheidung.
