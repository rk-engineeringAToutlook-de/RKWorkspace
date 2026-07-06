# PDF Frame Renderer Decision

Status: MA010.06

## Entscheidung fuer MA010

MA010 finalisiert noch keinen Produkt-Renderer. MA010 finalisiert die Renderer-Abstraktion und markiert den aktuellen Dev Renderer explizit als Placeholder.

Aktueller Status:

```text
RendererName: MetadataPreviewDevRenderer
FrameFormat: Placeholder
IsPlaceholder: true
RendererStatus: RendererBlocked
```

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
| Windows PDF Preview Handler | Windows-only candidate | Windows-nah, eventuell schnell fuer lokale Piloten | Nicht plattformneutral, Automatisierung/Sandbox unklar |
| WebView/Edge owner-side only | Spike candidate | Schnell sichtbar, gute Anzeige | darf nie Datei an Guest uebergeben, Headless/Automation offen |
| SkiaSharp/PDF | Evaluation candidate | Cross-platform Grafikpfad | PDF-Rendering-Faehigkeit und Packaging pruefen |
| Commercial Renderer | Later evaluation | Support und Qualitaet moeglich | Lizenz, Kosten, Vendor Lock-in |

## Vorlaeufige Richtung

Fuer den ersten echten Renderer-Spike ist PDFium der wahrscheinlich beste Kandidat, wenn Packaging und Security-Update-Pfad sauber geloest werden.

MuPDF bleibt fachlich stark, aber Lizenz/Einbindung muessen vor Produktnutzung sauber entschieden werden.

## Pflicht fuer jeden echten Renderer

- Owner-side oder strictly FrameOnly.
- `RenderResult` muss `RendererName` und `IsPlaceholder=false` liefern.
- Guest bekommt nie Originaldatei oder Originalpfad.
- Render-Cache muss `FrameCachePolicy` respektieren.
- Fehler muessen kontrolliert auf Placeholder/Denied fallen, nicht auf Dateimaterialisierung.
