# Linux Platform Tasks

## Plattformziel

Linux wird als Desktop-, Headless- und Industrie-Ablage vorbereitet.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.Linux`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native Linux-App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceProximityProvider`
- `ISurfaceSecurityContext`

## Build-Hinweise

Der Stub ist dokumentarisch und bricht den Windows-Build nicht. Native Linux-Implementierung muss X11/Wayland getrennt bewerten.

## Berechtigungen und Grenzen

- X11/Wayland Unterschiede
- Portals
- Screenshot/Capture-Grenzen
- Overlay-Moeglichkeiten je Desktop Environment
- Haptik wahrscheinlich eingeschraenkt

## Aktuelle Blocker

- kein Linux-Agent
- keine Overlay-Entscheidung
- keine Portal-Policy

## Naechster Codex-Auftrag

Minimalen Linux-Agent und FrameGuestSurface-Handoff fuer FrameOnly planen.

## GitHub und Context Pack

Linux-Arbeit laeuft ueber Feature-Branch, GitHub und Context-Pack. Keine Linux-Sondersemantik fuer Ownership.
