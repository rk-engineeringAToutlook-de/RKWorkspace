# Next Codex Actions

Status: Draft  
Datum: 2026-07-05

## Windows-Codex Naechster Auftrag

Baue Windows PDF Owner + Windows Local Guest Surface End-to-End mit echter PDF sichtbar.

Ziel:

- Windows bleibt Owner.
- Guest Surface zeigt Frame.
- Keine Originaldatei wird auf Guest geschrieben.
- Glass Edge startet FrameOnly-Pfad.
- No File Ingress bleibt PASS.

## macOS-Codex Naechster Auftrag

Baue macOS Frame Guest Surface, das RKWP Frame anzeigen kann.

Zu nutzen:

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`

## iOS/iPadOS Ueber macOS/Xcode-Codex Naechster Auftrag

Baue iPad/iPhone RK Workspace Surface App in Xcode, die RKWP Frame anzeigen kann und Haptik vorbereitet.

Zu nutzen:

- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`

## Android Spaeter

Baue Android RK Workspace Surface App mit FrameGuestSurface, Haptik und Glass Edge.

Zu nutzen:

- `Docs/Codex/PlatformTasks/Android.md`
- `Docs/Platform/Android_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Android/`

## Linux Spaeter

Baue Linux FrameGuestSurface unter Beruecksichtigung von Wayland/X11/Portals.

Zu nutzen:

- `Docs/Codex/PlatformTasks/Linux.md`
- `Docs/Platform/Linux_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/`

## Hardware/Dongle Spaeter

Definiere Dongle Hardware Requirements fuer Ablage-Anker mit BLE/UWB/USB.

Zu nutzen:

- `Docs/AblageAnchorDongle.md`
- `Docs/AblageProximityAndDistance.md`
- `Docs/Protocol/RKWP_TransportProfiles.md`

## Reihenfolge

1. Windows Local End-to-End, weil alle Komponenten lokal kontrollierbar sind.
2. macOS Guest Surface, weil dies der erste echte Cross-Device-Frame ist.
3. iPad Surface App, weil Tablet als digitale Ablage fuer den Owner wichtig ist.
4. Produktive Security und Trust erst nach sichtbarem FrameOnly-Pfad.
