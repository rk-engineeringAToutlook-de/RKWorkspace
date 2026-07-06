# PDF Renderer Product Decision

Status: MA013.31  
Datum: 2026-07-06

## Ziel

PDF Rendering darf nicht dauerhaft unklar bleiben. RK Workspace braucht einen Produktpfad, der PDF-Seiten als Frames erzeugt, ohne dem Guest die Originaldatei, den Owner-Pfad oder kopierte PDF-Bytes zu geben.

## Kriterien

- FrameOnly und No File Ingress bleiben verbindlich.
- Renderer laeuft Owner-seitig oder in einer isolierten Renderer-Sandbox.
- Multi-Page, Zoom, Scroll, Tiles und Annotation-ChangeSets muessen moeglich sein.
- Windows, macOS, iOS/iPadOS, Android und Linux duerfen nicht in getrennte Produktlogiken zerfallen.
- Lizenz, Security Updates, Packaging und Crash-Isolation muessen produktfaehig sein.

## Kandidaten

| Kandidat | Staerke | Risiko | Bewertung |
| --- | --- | --- | --- |
| PDFium | Starkes Rendering, breite Nutzung, permissiver Lizenzpfad in den offiziellen Quellen | Native Packaging, Update-Kette, Sandbox muss selbst gebaut werden | Bevorzugter Produktkandidat |
| MuPDF | Sehr gute Qualitaet, schnell, kommerzielle Option vorhanden | AGPL/commercial Entscheidung, Vendor-/Lizenzreview noetig | Technisch stark, rechtlich klaeren |
| Native Platform Preview | Gute Plattformintegration, besonders Apple PDFKit | Unterschiedliche Faehigkeiten je Plattform, kein einheitlicher Kern | Guest-UI-Kandidat, nicht alleiniger Kern |
| Commercial Renderer | Support, SLA, Security Advisories moeglich | Kosten, Vendor Lock-in, Lizenzbedingungen | Spaeterer Enterprise-Kandidat |
| WebView owner-side | Schneller Prototyp, HTML/Canvas-UI moeglich | Sandbox/Headless/Automatisierung unklar, darf keine Datei an Guest geben | Nur Spike, kein bevorzugter Produktkern |
| Poppler CLI | Bereits als Dev-Renderer nutzbar | Externe Tools, Packaging, Update-Kette | Development- und Test-Renderer |

## Quellenstand

- PDFium-Lizenz/Quelllage: `https://pdfium.googlesource.com/pdfium/+/main/LICENSE`
- MuPDF Produkt-/Lizenzlage: `https://mupdf.com/` und `https://github.com/ArtifexSoftware/mupdf`
- Apple PDFKit Plattformfaehigkeit: `https://developer.apple.com/documentation/pdfkit`
- Microsoft WebView2 Plattformmodell: `https://learn.microsoft.com/en-us/microsoft-edge/webview2/`

## Entscheidung fuer MA013

Fuer den Pilot bleibt Poppler der Development-Renderer, weil er bereits echte PNG-Frames erzeugt und No File Ingress pruefbar macht.

Fuer den Produktpfad wird PDFium als bevorzugter Kandidat gefuehrt. Der naechste Schritt ist kein Austausch des Renderers, sondern eine Sandbox-faehige Renderer-Abstraktion:

```text
Owner Original PDF
  -> Renderer Sandbox
  -> PageFrame / TileFrame
  -> RKWP FrameUpdate
  -> Guest Surface
```

## Blocker vor Produktfreigabe

- Lizenzreview fuer PDFium/MuPDF/commercial.
- Security-Update-Prozess fuer Renderer-Binaries.
- Prozessisolation und Crash-Recovery.
- Kein Netzwerk im Renderer-Prozess.
- Keine persistenten Originalbytes in Guest- oder Frame-Cache.
- Malformed-PDF-Fuzzing und Regression.

## Empfehlung

PDFium wird als Produktkandidat priorisiert. MuPDF bleibt technischer Fallback, falls PDFium Packaging oder Renderqualitaet nicht ausreichen. Native Preview bleibt UI-Ergaenzung auf Plattformen, aber nicht die alleinige RKWP-Renderer-Basis.
