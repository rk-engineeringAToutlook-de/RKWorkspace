# Real PDF Frame Viewer

Status: Draft  
Datum: 2026-07-05

## Ziel

Der PDF-Frame-Pfad soll eine echte PDF als ursprungsgebundenen Frame auf einer Gastablage zeigen, ohne dass die Gastablage eine freie PDF-Datei, einen Originalpfad oder kopierte PDF-Bytes erhaelt.

## Aktueller Stand

MA007.05 verbessert den bisherigen Smoke-Pfad:

- Sample-PDF wird als echtes PDF validiert.
- Owner bleibt Eigentuemer.
- CarryLease ist aktiv.
- FrameSession ist aktiv.
- Owner ist waehrend der Lease logisch gesperrt.
- Guest erhaelt eine sichere Frame-Repraesentation.
- Scroll und Zoom sind als Frame-Faehigkeiten vorbereitet.
- Rueckgabe und Recovery funktionieren.
- No File Ingress wird im Smoke geprueft.

## Renderer-Status

Ab MA011.04 wird der PDF-Inhalt im Development-Pfad erstmals als echte Seite gerendert, sofern Poppler verfuegbar ist. Die Renderer-Abstraktion umfasst:

- `IPdfFrameRenderer`
- `PdfFrameRenderRequest`
- `PdfFrameRenderResult`
- `PdfFrameRenderOptions`
- `PdfFrameRendererDiagnostics`
- `PdfFrameRendererCapabilities`
- `PdfFrameRendererStatus`
- `FrameFormat`

Der aktuelle echte Development Renderer heisst `PopplerPdfFrameRenderer` und meldet:

```text
FrameFormat: PngFrame
IsPlaceholder: false
RendererStatus: Rendered
```

Damit ist die Frame- und Ownership-Mechanik erstmals mit einer echten ersten Seitenvorschau testbar. Der visuelle Produkt-Viewer ist trotzdem noch nicht final, weil Packaging, Sandbox, Update-Pfad und Plattformintegration noch entschieden werden muessen.

Renderer-Blocker:

- Wenn Poppler fehlt, wird der Blocker explizit gemeldet.
- Der Renderer muss Owner-seitig arbeiten oder strikt FrameOnly bleiben.
- Der Guest darf keine Originaldatei und keine freie PDF-Kopie erhalten.
- Lizenz, Packaging und Plattformpfad muessen dokumentiert sein.

## Renderer-Kandidaten

- Poppler CLI: aktueller Development Renderer fuer echte PNG-Frames.
- PDFium: bevorzugter Kandidat fuer den Produkt-Renderer-Spike, Lizenz und Packaging pruefen.
- MuPDF: leistungsfaehig, Lizenz besonders sorgfaeltig pruefen.
- Windows PDF Preview Handler: Windows-nah, aber nicht plattformneutral.
- WebView2/Edge: nur Owner-seitig als Renderer denkbar, nicht als Dateiuebergabe an Guest.
- SkiaSharp/PDF: pruefen, ob Darstellung ohne Gast-Dateimaterialisierung stabil moeglich ist.
- Commercial Renderer: spaeter nur mit sauberem Lizenz-/Update-Modell.

Details:

```text
Docs/Frame/PdfFrameRendererDecision.md
Docs/Frame/PdfFrameRendererAbstraction.md
```

## Sicherheitsregel

Auch bei echtem Rendering bleibt:

- Originalablage besitzt die PDF.
- Zielablage sieht nur FrameUpdates.
- Zielablage bekommt keine PDF-Datei.
- Zielablage bekommt keinen Originalpfad.
- Zielablage bekommt keine kopierten PDF-Bytes als Datei.

## Smoke-Test

```powershell
.\tools\run-pdf-frame-smoke.ps1
```

Der Smoke prueft Sample-PDF, CarryLease, FrameSession, FrameRepresentation, `FrameFormat: PngFrame` oder klaren Renderer-Blocker, GuestHasPdfFile `NO`, OriginalFileBytes `NO`, Rueckgabe, Recovery und `NoFileIngress: SUCCESS`.

Ab MA009.08 prueft der Smoke zusaetzlich die testbare Owner/Guest-State-UX:

- Owner sichtbar: `wartet auf Rueckgabe`
- Guest sichtbar: `liegt hier im Frame`
- Rueckgabe sichtbar: `zurueckgegeben`
- Recovery sichtbar: `wieder verfuegbar`
- Revocation sichtbar: `nicht verfuegbar`
- Ablauf sichtbar: `Verbindung verloren`
- sichtbare Zustandstexte enthalten keine verbotenen Produktwoerter

## Naechster Schritt

Der naechste echte Viewer-Schritt ist die Anzeige des PNG-Frames in der nativen Gastoberflaeche und danach die Produktentscheidung fuer PDFium/MuPDF/Poppler-Packaging.
