# PDF Frame Performance

Status: MA013.37  
Datum: 2026-07-06

## Ziel

PDF Frames muessen produktnah messbar werden: erste Seite, naechste Seite, Tile-Generation, Framegroesse und Speicher.

## Tool

```powershell
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\run-rkwp-perf.ps1 -Iterations 100
```

Implementierung:

```text
src/Tools/RKWorkspace.RkwpPerfHarness/
```

## Neue Messwerte

| Messwert | Zweck |
| --- | --- |
| `PdfRenderFirstPageAverageMs` | Zeit bis erste Seite als Frame bereit ist |
| `PdfRenderNextPageAverageMs` | Zeit fuer Folgeseite oder Placeholder |
| `PdfTileGenerationAverageMs` | Zeit fuer Viewport/Tile/DirtyRegion-Erzeugung |
| `PdfFrameSizeBytesAverage` | Groesse der FrameUpdate-/Tile-Metadaten |
| `MemorySnapshotBytesAverage` | lokaler Speicher-Snapshot waehrend Smoke |

## Interpretation

Der Smoke-Test ist noch keine Endperformance. Er beweist:

- FrameUpdates bleiben klein.
- Tiles entstehen ohne File Ingress.
- Renderer-Status wird sichtbar.
- Die Performance-Reports werden reproduzierbar in `logs/perf/` abgelegt.

## Grenzen

Noch offen:

- echte Cross-Device-Latenz
- Produkt-TLS
- Renderer-Prozesssandbox
- parallele Multi-Frame-Sessions
- echte mobile Guest-UI-Renderzeit

## Gate

Vor jedem Renderer-Wechsel muessen `run-rkwp-perf.ps1 -SmokeTest`, `run-pdf-frame-smoke.ps1` und `run-rkwp-tests.ps1` gruen bleiben.
