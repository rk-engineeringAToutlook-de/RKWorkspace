# Linux Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-LINUX-001
Status: Draft
Datum: 2026-07-05

## Ziel

Linux wird als Desktop-, Headless- und Industrie-Ablage vorbereitet.

Der erste Pfad ist eine minimale FrameGuestSurface oder ein Agent-Prozess, der RKWP FrameOnly respektiert.

## Rollen

- Desktop-Ablage
- Industrie-/Headless-Ablage
- FrameGuestSurface
- Audit/Recovery-Knoten

## X11 vs Wayland

Linux-Codex muss getrennt bewerten:

- X11 Overlay-Moeglichkeiten
- Wayland-Sicherheitsgrenzen
- Portals fuer Screenshot/Capture
- Desktop Environment Unterschiede

## No File Ingress

Pflicht:

- keine Originaldatei in die Gastablage kopieren
- FrameOnly als Default
- OriginalOwned respektieren
- Audit fuer kritische Umgebungen

## Agent-Prozess

Ein Linux-Agent kann zuerst ohne Overlay starten:

- SurfaceIdentity
- RKWP DevTransport spaeter
- FrameGuestSurface Stub
- Audit und Recovery

## Packaging spaeter

Noch offen:

- AppImage
- Flatpak
- deb/rpm
- systemd user service
- Industrie-Deployment

## Offene Blocker

- keine Linux-Umgebung in diesem Windows-Thread
- keine Wayland/X11 Entscheidung
- keine Portal-Policy
- keine Packaging-Strategie
