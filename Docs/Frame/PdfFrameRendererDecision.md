# PDF Frame Renderer Decision

Status: MA011.04

## Entscheidung fuer MA011.04

MA011.04 fuehrt einen ersten echten Owner-seitigen Development-Renderer ein.

Aktueller Status:

```text
RendererName: PopplerPdfFrameRenderer
FrameFormat: PngFrame
IsPlaceholder: false
RendererStatus: Rendered
```

Der Renderer nutzt lokal vorhandene Poppler-Binaries (`pdfinfo`, `pdftoppm`) und erzeugt die erste Seite als PNG-Frame. Die Gastablage erhaelt weiterhin keine PDF-Datei, keinen Originalpfad und keine kopierten Originalbytes.

## Bewertungskriterien

Jeder Kandidat wird gegen diese Kriterien bewertet:

- Lizenz.
- Offline-Faehigkeit.
- Sicherheit.
- Performance.
- Windows.
- macOS.
- iOS/iPadOS.
- Android.
- Linux.
- Headless moeglich.
- keine Dateiuebergabe an Guest.
- FrameOnly kompatibel.

## Kandidaten

| Kandidat | Status | Staerke | Risiko |
| --- | --- | --- | --- |
| PDFium | Preferred candidate | Starkes Rendering, weit verbreitet, plattformnah | Packaging, Native-Binaries, Security-Updates |
| MuPDF | Candidate | Sehr gute Qualitaet und Performance | Lizenz muss sorgfaeltig geklaert werden |
| Poppler CLI | Development renderer | Headless, lokal verfuegbar, rendert echte PNG-Frames | Externe Binaries, Packaging/Update-Pfad fuer Produkt offen |
| Windows PDF Preview Handler | Windows-only candidate | Windows-nah, eventuell schnell fuer lokale Piloten | Nicht plattformneutral, Automatisierung/Sandbox unklar |
| WebView/Edge owner-side only | Spike candidate | Schnell sichtbar, gute Anzeige | darf nie Datei an Guest uebergeben, Headless/Automation offen |
| SkiaSharp/PDF | Evaluation candidate | Cross-platform Grafikpfad | PDF-Rendering-Faehigkeit und Packaging pruefen |
| Commercial Renderer | Later evaluation | Support und Qualitaet moeglich | Lizenz, Kosten, Vendor Lock-in |

## Vorlaeufige Richtung

Fuer den ersten echten Development-Renderer ist Poppler ausreichend, weil es headless eine PNG-Seite erzeugen kann und No File Ingress nicht verletzt.

Fuer den Produktpfad bleibt PDFium der wahrscheinlich beste Kandidat, wenn Packaging und Security-Update-Pfad sauber geloest werden.

MuPDF bleibt fachlich stark, aber Lizenz/Einbindung muessen vor Produktnutzung sauber entschieden werden.

## Pflicht fuer jeden echten Renderer

- Owner-side oder strictly FrameOnly.
- `RenderResult` muss `RendererName` und `IsPlaceholder=false` liefern.
- Guest bekommt nie Originaldatei oder Originalpfad.
- Render-Cache muss `FrameCachePolicy` respektieren.
- Fehler muessen kontrolliert auf Placeholder/Denied fallen, nicht auf Dateimaterialisierung.

## Blocker, falls Poppler fehlt

Wenn `pdfinfo` oder `pdftoppm` auf einer Maschine fehlen, ist der Renderer-Blocker explizit:

```text
Poppler pdfinfo/pdftoppm not found. Install Poppler or provide RKWS_POPPLER_BIN.
```

Der Smoke-Test akzeptiert nur einen echten `PopplerPdfFrameRenderer` oder diesen klaren Blocker.
