# Linux Implementation Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP186 beschreibt den Linux-Starter.

## .NET Agent

Der erste Linux Agent kann als .NET Konsolenprozess starten:

- RKWP Client/Host
- Config Loader
- Diagnostics
- Frame Guest Surface Stub

## UI Optionen

Zu pruefen:

- GTK
- Qt
- Avalonia
- WebView nur fuer Lab, nicht Produktziel

## Wayland/X11

Wayland:

- restriktivere Capture-/Overlay-Rechte
- Portal APIs beachten

X11:

- technisch einfacher, aber Security anders

## Frame Guest

Linux muss FrameOnly respektieren:

- keine Originaldatei schreiben
- Frame Cache memory-only bevorzugen
- Return/Recovery sichtbar machen

## No File Ingress

Linux darf nicht durch tmp-Dateien oder Viewer-Export die Guest-Ablage zur freien Datei machen.

## Nicht-Ziele

- kein produktiver Daemon in AP186
- kein globaler Input Hook
- keine Desktop-Environment-spezifische Festlegung
