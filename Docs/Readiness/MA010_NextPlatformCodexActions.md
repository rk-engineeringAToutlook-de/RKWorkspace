# MA010 Next Platform Codex Actions

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma010-real-cross-device-frame-foundation`

## Windows-Codex

```text
Baue sichtbaren Windows PDF Owner/Guest Pilot weiter aus und stabilisiere Frame UI.
```

Ziel:

- sichtbarer Owner/Guest Pilot mit echter PDF
- bessere Frame UI statt reinem CLI/Placeholder-Gefuehl
- No File Ingress weiter als Gate
- Rueckgabe und Recovery sichtbar
- Windows Agent Dev Host nur als Laborpfad, keine produktive Installation

Startpunkte:

- `src/Tools/RKWorkspace.WindowsPdfFramePilot/`
- `src/Tools/RKWorkspace.WindowsLocalFrameE2E/`
- `src/Frame/RKWorkspace.Frame.Pdf/`
- `tools/run-windows-pdf-frame-pilot.ps1`
- `tools/run-windows-local-frame-e2e.ps1`

## macOS-Codex

```text
Baue macOS Frame Guest Surface mit RKWP DevLan Client.
```

Ziel:

- native macOS Guest Surface
- RKWP DevLan Client
- AblageIdentity und DevPairing
- FrameOnly PDF Anzeige
- keine PDF-Datei, kein Originalpfad, keine Originalbytes
- Rueckgabe und Recovery sichtbar

Startpunkte:

- `release/handoff/macOS_Codex_MA009_FrameGuestSurface.md`
- `release/handoff/WindowsToMac_MA009_Handoff.md`
- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`

## iOS/iPadOS Ueber macOS/Xcode-Codex

```text
Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
```

Ziel:

- native iPad/iPhone Surface App
- FrameOnly Anzeige
- Touch/Haptik vorbereitet
- USB-Test mit echtem iPad/iPhone
- keine globale App-Erfassung
- keine Dateiablage des Originals

Startpunkte:

- `release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md`
- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/`

## Android-Codex Spaeter

```text
Baue Android Frame Guest Surface mit RKWP DevTransport.
```

Ziel:

- Android Surface Host
- Touch/Haptik-Plan
- FrameOnly Anzeige
- No File Ingress
- Android Storage Access Framework sauber begrenzen

Startpunkte:

- `Docs/Codex/PlatformTasks/Android.md`
- `Docs/Platform/Android_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Android/`

## Linux-Codex Spaeter

```text
Baue Linux Frame Guest Surface unter Wayland/X11-Beruecksichtigung.
```

Ziel:

- Linux Surface Host
- Wayland/X11 Unterschiede dokumentieren und implementieren
- FrameOnly Anzeige
- No File Ingress
- Industrie-/Desktop-Szenarien spaeter pruefen

Startpunkte:

- `Docs/Codex/PlatformTasks/Linux.md`
- `Docs/Platform/Linux_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/`

## Hardware/Dongle Spaeter

```text
Definiere Dongle Hardware MVP fuer Ablage-Anker mit BLE/UWB/USB.
```

Ziel:

- Ablage-Anker statt Geraete-Mittelpunkt
- BLE/UWB fuer Naehe und Richtung
- USB fuer Energie/Provisioning
- Trust/Identity-Anker
- keine Firmware-Implementierung vor finaler Requirements-Freigabe

Startpunkte:

- `Docs/AblageAnchorDongle.md`
- `Docs/AblageProximityAndDistance.md`
- `Docs/Protocol/RKWP_TransportProfiles.md`
- `Docs/Roadmap/RKWorkspace_Roadmap.md`

## Empfohlene Reihenfolge

1. Windows Pilot sichtbar stabilisieren.
2. macOS Guest Surface bauen.
3. Windows Owner zu macOS Guest im Labor testen.
4. iPad/iPhone Surface App ueber Xcode bauen.
5. Windows Owner zu iPad/iPhone Guest testen.
6. PDF Renderer produktionsnaeher entscheiden.
7. Security von Dev/Lab zu Production haerten.
8. Danach Android/Linux und Dongle.
