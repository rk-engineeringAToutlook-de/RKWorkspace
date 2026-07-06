# RKWP Performance Baseline

Status: Draft  
Datum: 2026-07-06

## Ziel

MA009.09 legt die erste RKWP Transport/Frame Performance Baseline an.

Diese Baseline ist noch keine Produktoptimierung. Sie misst lokal reproduzierbar:

- FrameUpdate-Groesse
- FrameUpdate-Frequenz
- DevTransport Roundtrip-Latenz
- Heartbeat-Latenz
- FrameOpen-Zeit
- FrameReturn-Zeit
- Recovery-Zeit
- No File Ingress Overhead
- PDF-Ladezeit
- PDF-Renderzeit, falls ein Renderer vorhanden ist
- PDF-Renderzeit erste Seite
- PDF-Renderzeit naechste Seite
- PDF-Tile-Generation
- PDF-Framegroesse
- Speicher-Snapshot

## Tool

```powershell
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\run-rkwp-perf.ps1 -PdfPath samples\Objects\Rechnung.pdf
.\tools\run-rkwp-perf.ps1 -Iterations 100
```

Implementierung:

```text
src/Tools/RKWorkspace.RkwpPerfHarness/
```

Reports:

```text
logs/perf/rkwp-perf-<timestamp>.json
logs/perf/rkwp-perf-<timestamp>.md
```

`logs/` ist absichtlich nicht versioniert.

## Smoke-Kriterium

Der Smoke-Test muss melden:

- `Iterations: 10`
- `PerfSamples: OK`
- `NoFileIngress: SUCCESS`
- `PdfRenderFirstPageAverageMs`
- `PdfRenderNextPageAverageMs`
- `PdfTileGenerationAverageMs`
- `PdfFrameSizeBytesAverage`
- `MemorySnapshotBytesAverage`
- `RESULT: SUCCESS`
- JSON-Report existiert
- Markdown-Report existiert

## Erste lokale Smoke-Baseline

Lauf vom 2026-07-06:

| Metric | Smoke-Wert |
| --- | ---: |
| Iterationen | 10 |
| FrameUpdate bytes avg | 629.0 |
| FrameUpdate frequency Hz | ca. 4323 |
| DevTransport roundtrip avg ms | ca. 0.783 |
| Heartbeat latency avg ms | ca. 1.334 |
| FrameOpen avg ms | ca. 0.137 |
| FrameReturn avg ms | ca. 0.016 |
| Recovery avg ms | ca. 0.034 |
| No File Ingress overhead avg ms | ca. 0.003 |
| PDF render avg ms | 0.000 |
| PDF render first page avg ms | ab MA013.37 gemessen |
| PDF render next page avg ms | ab MA013.37 gemessen |
| PDF tile generation avg ms | ab MA013.37 gemessen |
| PDF frame size bytes avg | ab MA013.37 gemessen |
| Memory snapshot bytes avg | ab MA013.37 gemessen |

RendererStatus:

```text
RendererBlocked
```

## Interpretation

Die Messung beweist nur die lokale Development-Strecke:

- NamedPipeDev ist schnell genug fuer Baseline-Smokes.
- FrameUpdate-Objekte sind klein, solange nur MetadataPreview genutzt wird.
- No File Ingress erzeugt lokal keinen messbaren Overhead.
- Echte PDF-Seitenrenderzeit ist noch nicht gemessen, weil der Renderer weiterhin blockiert ist.

## Grenzen

Noch nicht gemessen:

- echter Netzwerktransport
- macOS/iOS/iPadOS Guest Surface
- echte PDF-Seitenbilder
- Haptik- oder UI-Rendering-Zeit
- TLS/mutual-auth Produktionspfad
- mehrere parallele Frames

## Empfehlung

Diese Baseline ist das Pflichtinstrument vor Optimierung. Erst wenn FrameUpdates echte Seitenbilder tragen und macOS/iPad echte Gastablagen sind, wird die Performance-Baseline produktnah.
