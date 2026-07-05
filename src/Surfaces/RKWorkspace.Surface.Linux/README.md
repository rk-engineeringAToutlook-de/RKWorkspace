# RKWorkspace.Surface.Linux

Status: Prepared stub

Linux wird als Desktop- und Industrie-Surface vorbereitet. Der Startpfad ist bewusst minimal: Maus-/Tastaturaktivierung, Frame-Presentation und spaeter lokale Proximity.

MA007.00 implementiert noch keine Linux-APIs.

Pflichtsemantik:

- RKWP bleibt transportneutral.
- Linux-Surface kopiert keine Originaldateien in FrameOnly.
- Headless- und Industrie-Szenarien muessen Audit und Recovery unterstuetzen.

## Linux-Pfade

- X11
- Wayland
- Portals
- transparente Overlays mit klar dokumentierten Grenzen
- Agent fuer Industrie-/Headless-Szenarien

## Naechster Plattformauftrag

Minimalen Linux Surface Host planen, ohne den Windows-Build zu beeinflussen.
