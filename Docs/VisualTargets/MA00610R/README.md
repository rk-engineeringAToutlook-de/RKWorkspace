# MA006.10R Visual Targets

Dokument-ID: RKWS-MA006-10R-VISUAL-TARGETS
Version: 1.1.0
Status: Accepted
Datum: 2026-07-04

## Zweck

Dieses Verzeichnis enthaelt exportierte Zielbilder des Living-Lens-Spikes.

Sie sind keine finalen Produktgrafiken.

Sie pruefen, ob der neue Spike die verworfene UI-Kreis-/Bubble-Richtung sichtbar verlaesst.

Die exportierten Bilder besitzen weiterhin einen Testhintergrund, damit Material, Schatten und Oeffnung in Dateien beurteilbar sind. Die laufende Overlay-App nutzt dagegen eine Per-Pixel-Alpha-Schicht ohne Magenta-/Color-Key-Hintergrund; der echte Desktop bleibt sichtbar.

## Erzeugung

```powershell
.\tools\run-living-lens.ps1 -ExportFrames
```

## Dateien

| Datei | Inhalt |
| --- | --- |
| `real_bubble_lens.png` | Real Bubble Lens. |
| `glass_lens.png` | Glass Lens. |
| `water_surface_lens.png` | Water Surface Lens. |
| `wormhole_lens.png` | Wormhole Lens. |
| `gravity_lens.png` | Gravity Lens. |
| `lens_appearance_sequence.png` | langsames Erscheinen. |
| `lens_opening_sequence.png` | Oeffnung der Linse. |
| `lens_absorption_sequence.png` | Ding wird in die Linse gezogen. |
| `target_emergence_sequence.png` | Ghost kommt auf der Zielablage heraus. |
| `thing_in_hand.png` | digitales Ding mit Griffwirkung. |

## Bewertungsfrage

```text
Wirkt es naeher an lebendigem Material als an UI?
```

Zweite Bewertungsfrage:

```text
Bleibt der Desktop der Raum, und ist die Blase nur Material darin?
```
