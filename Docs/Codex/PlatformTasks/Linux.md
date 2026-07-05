# Linux Platform Tasks

## Plattformziel

Linux wird als Desktop-, Headless- und Industrie-Ablage vorbereitet.

Der erste Linux-Pfad ist eine minimale FrameGuestSurface oder ein Agent-Prozess mit klaren Grenzen fuer X11, Wayland und Portals.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.Linux`
- Starter Kit: `Docs/Platform/Linux_SurfaceStarterKit.md`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native Linux-App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceProximityProvider`
- `ISurfaceSecurityContext`
- `ISurfaceInputChannel`
- `SurfacePlatform.Linux`

## Build-Hinweise

Der Stub ist dokumentarisch und bricht den Windows-Build nicht. Native Linux-Implementierung muss X11/Wayland getrennt bewerten.

## Berechtigungen und Grenzen

- X11 vs Wayland
- Portals
- Screenshot/Capture-Grenzen
- Overlay-Moeglichkeiten je Desktop Environment
- Headless/Industrie-Agent
- Packaging spaeter
- Haptik wahrscheinlich eingeschraenkt

## RKWP-Pflicht

- FrameOnly als sicherer Default
- No File Ingress
- OriginalOwned respektieren
- Audit und Recovery fuer Industriepfade

## Aktuelle Blocker

- kein Linux-Agent
- keine Overlay-Entscheidung
- keine Portal-Policy
- keine Packaging-Strategie

## Naechster Codex-Auftrag

Minimalen Linux-Agent und FrameGuestSurface-Handoff fuer FrameOnly planen.

## GitHub und Context Pack

Linux-Arbeit laeuft ueber Feature-Branch, GitHub und Context-Pack. Keine Linux-Sondersemantik fuer Ownership.
