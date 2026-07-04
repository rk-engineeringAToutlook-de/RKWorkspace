# MA006.12 Extreme Tunnel / Bubble FX

Dokument-ID: RKWS-MA006-12-EXTREME-FX
Version: 1.0.0
Status: Accepted
Datum: 2026-07-04

## Ziel

MA006.12 ist ein Spike fuer maximale Wirkung. Dieser Stand ist kein finaler Produktstil. Er soll die obere Grenze fuer Glas, Wasser, Tunnel, Raumverzerrung, Absorption und mobile Ablage-Wahrnehmung zeigen.

## Desktop-Pfade

Es gibt zwei testbare Desktop-Pfade:

- `.\tools\run-living-lens.ps1`
- `.\tools\run-gpu-lens.ps1`

Der Living-Lens-Pfad liefert schnelle Visual-Target-Frames und einen stabilen Smoke-Test. Der GPU-Lens-Pfad ist der wichtigere Wahrnehmungspfad fuer Desktop-Refraction, echte Desktop-Samples, PixelShader-Layer und transparentes Overlay.

## Extreme Presets

Tasten im manuellen GPU-Test:

- `1`: Extreme Glass Bubble.
- `2`: Extreme Water Lens.
- `3`: Extreme Wormhole Tunnel.
- `4`: Extreme Gravity Well.
- `5`: Extreme Portal Absorption.
- `A`: Absorption abspielen.
- `O`: Linse oeffnen.
- `T`: Timing wechseln.
- `+`: Intensitaet erhoehen.
- `-`: Intensitaet verringern.
- `D`: Debug ein/aus.
- `R`: Reset.
- `Esc`: Beenden.

Debug ist standardmaessig aus.

## Intensitaet

`EffectIntensity` besitzt die Werte 1 bis 5. Fuer diesen Spike startet die Wirkung bewusst stark. Das Ziel ist nicht Zurueckhaltung, sondern ein klares Gefuehl fuer Tiefe und Sog.

## Timing

Absorption wird mit vier Timings getestet:

- 600 ms.
- 1200 ms.
- 1800 ms.
- 2400 ms.

Der Owner entscheidet spaeter, welches Timing sich natuerlich anfuehlt.

## Mobile Spatial Surface

Der mobile Einstieg laeuft ueber:

```powershell
.\tools\run-mobile-spatial-surface.ps1
```

Das Script gibt die mobile URL aus:

```text
http://<ip>:<port>/mobile
```

Auf dem Tablet oder iPhone erscheinen die Linsen nicht sofort. Erst ein langer Touch aktiviert `MobileSpatialMode`.

Danach erscheinen Ablagen als raeumliche Linsen:

- nahe Ablage: groesser, staerker, Name lesbar.
- entfernte Ablage: kleiner, ruhiger, Name kaum sichtbar.
- sehr nahe Ablage: Linse oeffnet sich und zeigt `Hier ablegen`.

Haptik wird ueber `navigator.vibrate` versucht. Wenn die Plattform sie nicht anbietet, uebernimmt optische Haptik ohne Fehler.

## Visual Targets

Export:

```powershell
.\tools\run-living-lens.ps1 -ExportFrames
```

Zielordner:

```text
Docs/VisualTargets/MA00612/
```

## Renderer-Grenze

Die aktuelle Technik beweist Steuerung, Raumlogik, Timing, Visual Targets und erste Materialwirkung.

Fuer das finale Zielgefuehl reicht sie allein nicht aus. Fuer wirklich hochwertige Glas-/Wasser-/Tunnelwirkung wird ein staerkerer nativer Shaderpfad benoetigt: Direct2D/Win2D oder Windows Composition mit aktiver Desktop-Refraction, Mesh-/Quad-Warp und besserem Material-Lighting.

## Verifikation

Automatisiert geprueft:

- Extreme FX Mode.
- fuenf Presets.
- Intensitaet.
- Timing-Varianten.
- Debug default hidden.
- Absorption, Scale, Distortion, Target Ghost.
- Visual Target Export.
- Mobile Surface.
- Gesture Mode.
- Mobile Linsen nach Geste.
- Distanzskalierung.
- Name Reveal.
- Haptik oder sauberer Fallback.
