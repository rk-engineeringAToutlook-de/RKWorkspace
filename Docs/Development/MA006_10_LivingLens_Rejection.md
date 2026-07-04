# MA006.10R Living Lens Renderer Reset

Dokument-ID: RKWS-MA006-10R-LIVING-LENS-REJECTION
Version: 1.1.0
Status: Accepted
Datum: 2026-07-04

## Entscheidung

Der bisherige Living-Lens-/Bubble-Pfad mit flachen Kreisen, lila Flaechen, gruenen Punkten oder UI-artigen Zielmarkierungen ist als Human-Experience-Pfad verworfen.

Diese Entscheidung betrifft das sichtbare Gefuehl, nicht die Shell-Architektur.

## Warum

Eine Ablage-Linse darf nicht wie ein Ziel, ein Button oder eine Statusanzeige wirken.

Sie muss wie lebendiges Material wirken:

- transparent.
- fein.
- lichtbrechend.
- reflektierend.
- raeumlich.
- subtil bewegt.
- am Rand der realen Ablage verankert.

Wenn die Linse nicht wie Material wirkt, entsteht kein Raum.

## Neuer Spike

Der neue isolierte visuelle Spike liegt in:

```text
src/Shell/RKWorkspace.Shell.LivingLens.Windows
```

Start:

```powershell
.\tools\run-living-lens.ps1
.\tools\run-living-lens.ps1 -SmokeTest
.\tools\run-living-lens.ps1 -ExportFrames
```

## Varianten

- `1`: Real Bubble Lens.
- `2`: Glass Lens.
- `3`: Water Surface Lens.
- `4`: Wormhole Lens.
- `5`: Gravity Lens.

## Bedienung

- `Ctrl + Alt + Space`: Ding greifen.
- Linke Maustaste auf `Rechnung.pdf`: Ding greifen.
- `O`: Oeffnung erneut abspielen.
- `A`: Lens Absorption erneut abspielen.
- `T`: Timing wechseln: 600 ms, 1200 ms, 1800 ms.
- `Esc`: beenden.

## Verboten Im Neuen Spike

- gruene Punkte.
- lila Flecken.
- flache UI-Kreise.
- Zielscheiben.
- Radar.
- Statuskarten.
- Dropzone-Rechtecke.
- Browser/WebView.
- technische Begriffe im Erlebnis.
- Magenta-/Color-Key-Hintergrund als scheinbarer Raum.
- automatische Absorption ohne Loslassen.

## Abgrenzung

Dieser Spike ist noch kein finaler Renderer. Die sichtbare Overlay-Flaeche nutzt jetzt Per-Pixel-Alpha und laesst den echten Desktop ohne Magenta-/Color-Key-Artefakte sichtbar. WinForms/GDI+ reicht damit fuer den korrigierten isolierten Smoke- und Frame-Export, aber nicht fuer das finale Zielgefuehl echter Desktop-Brechung, Shader, Blur und physikalischer Materialkomposition.

Die aktuelle Interaktion ist normativ:

- Naehe darf die Linse oeffnen.
- Wegbewegen muss die Linse beruhigen.
- Das Ding bleibt unter Kontrolle des Benutzers.
- Absorption startet erst beim Loslassen oder expliziten Replay.

Die ehrliche Renderer-Bewertung steht in:

```text
Docs/RenderingDecision_LivingLens.md
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-04 | Per-Pixel-Alpha, Color-Key-Verbot, Relax und kontrolliertes Loslassen ergaenzt. |
| 1.0.0 | 2026-07-04 | MA006.10R Living Lens Renderer Reset dokumentiert. |
