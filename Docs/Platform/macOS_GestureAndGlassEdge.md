# macOS Gesture und Glass Edge

Status: MA013.09 plan  
Datum: 2026-07-06

## Ziel

macOS soll spaeter Glass Edge und eine natuerliche Geste unterstuetzen. Fuer V1 bleibt der Frame-Pfad wichtiger als die finale UX.

## Geste

Zu pruefen:

- Drei-Finger-Langdruck.
- Drei-Finger-Drag.
- Trackpad Pressure.
- Fallback Hotkey.

Risiken:

- Mission Control.
- Spaces.
- systemweite Trackpad-Gesten.
- Accessibility-Freigaben.

## Glass Edge

Glass Edge erscheint am Rand, der zur naechsten Ablage zeigt. Es gibt nur eine aktive Kante.

V1 auf macOS:

- optional simulierte Kante im Testfenster.
- keine globale Desktop-Hook-Pflicht.
- FrameOnly-Flow bleibt massgeblich.

Spaeter:

- native transparente Kante.
- Haptik ueber Trackpad, sofern sinnvoll.
- bessere Entfernungserkennung.

## Ablauf

```text
Ding wird gehalten
naechste Ablage erkannt
Glass Edge wird sichtbar
Ding wird in Richtung Kante gefuehrt
FrameSessionOpen
FrameUpdate
liegt hier im Frame
```

## Fallback

Wenn die Geste mit macOS kollidiert, nutzt V1 einen Hotkey oder eine explizite Dev-Schaltflaeche im Testfenster.
