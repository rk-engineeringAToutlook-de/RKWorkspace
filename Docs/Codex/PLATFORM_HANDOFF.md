# Platform Handoff

Status: Draft
Datum: 2026-07-05

## Ziel

Plattform-Threads koennen ab MA007.00 parallel arbeiten, wenn sie die gemeinsame RKWP-Semantik nicht veraendern.

## Gemeinsame Pakete

- `RKWorkspace.Protocol`
- `RKWorkspace.Frame.Pdf`
- `RKWorkspace.Surface.Abstractions`

## Gemeinsame Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceHapticsProvider`
- `ISurfaceProximityProvider`
- `ISurfaceFramePresenter`
- `ISurfaceInputChannel`
- `ISurfacePlacementAdapter`
- `ISurfaceObjectAdapter`
- `ISurfaceSecurityContext`
- `GestureType`
- `GestureState`
- `SurfacePlatform`
- `SurfaceCapabilities`

## Plattformaufgaben

- Windows: native Frame-Presentation und Glass Edge mit realem Desktop.
- macOS: FrameGuestSurface mit No File Ingress, Trackpad-/Maus-Gesten, Glass Edge und Berechtigungspruefung.
- iOS/iPadOS: native Surface App ueber macOS/Xcode, TouchHold/Drei-Finger-Pruefung, Haptik, FrameView und Gegenkante.
- Android: native App mit Touch/Haptik, Sharesheet/Intent/Content URI, FrameView und Glass Edge.
- Linux: minimaler Desktop-/Industriepfad mit X11/Wayland/Portals-Bewertung.

## Unantastbar

Kein Plattformadapter darf im FrameOnly-Modus eine Originaldatei auf den Gast schreiben.

## macOS MA007.10 Handoff

macOS-Codex nutzt:

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`

Erster Auftrag: macOS Frame Guest Surface bauen, RKWP Frame anzeigen, keine Datei uebernehmen, Windows Owner Test vorbereiten.

## iOS/iPadOS MA007.11 Handoff

iOS/iPadOS-Codex laeuft ueber macOS-Codex und Xcode. Zu nutzen:

- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/`

Erster Auftrag: native iOS/iPadOS RK Workspace Surface App bauen, RKWP Frame anzeigen, Haptik vorbereiten, Glass Edge simulieren, keine Datei speichern.

## Android/Linux MA007.12 Handoff

Zu nutzen:

- `Docs/Codex/PlatformTasks/Android.md`
- `Docs/Platform/Android_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Android/`
- `Docs/Codex/PlatformTasks/Linux.md`
- `Docs/Platform/Linux_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/`

Android startet spaeter als native mobile Surface App. Linux startet spaeter als minimale FrameGuestSurface oder Agent-Prozess mit X11/Wayland/Portals-Bewertung.

## GitHub

GitHub ist die zentrale Synchronisation fuer Plattformzweige. Plattform-Threads arbeiten auf klar benannten Feature-Branches und werden erst nach Owner-Freigabe gepusht oder gemergt.
