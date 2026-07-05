# Platform Surface Foundation

Status: Accepted  
Datum: 2026-07-05

## Ziel

Windows, macOS, iOS und Android sollen spaeter dieselbe Human Experience tragen:

```text
Ding nehmen -> naechste Ablage sehen -> in gläserne Kante legen -> auf Gegenkante erscheinen
```

## Contracts

Die Shell bereitet folgende plattformneutralen Verträge vor:

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceHapticsProvider`
- `ISurfaceProximityProvider`
- `ISurfaceEdgeRenderer`
- `ISurfaceObjectCaptureAdapter`
- `ISurfacePlacementAdapter`

## Plattformen

Windows ist erster realer Testpfad.

macOS wird fuer Overlay, Edge Renderer, Trackpad-Gesten, Haptik und Berechtigungen vorbereitet.

iOS wird fuer mobile Ablage, Touch-Geste, Haptik, Displayrand-Kante und Sandbox-Grenzen vorbereitet.

Android wird fuer mobile Ablage, Touch-Geste, Haptik, Displayrand-Kante und Overlay-/Permission-Themen vorbereitet.

## Sichtbare Sprache

Sichtbar sind nur menschliche Begriffe wie `Ablage`, `Hier ablegen`, `Kommt an`, `Liegt hier`.

Technische Begriffe bleiben Diagnose- und Dokumentationsebene.
