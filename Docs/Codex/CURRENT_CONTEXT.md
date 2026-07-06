# Current Codex Context

Datum: 2026-07-06  
Branch: `feature/ma010-real-cross-device-frame-foundation`

## Aktueller Stand

MA010 ist im Abschluss AP040. Windows ist der aktive technische Referenzpfad. macOS ist als erste echte Gegenplattform vorbereitet. iOS/iPadOS wird ueber macOS-Codex und Xcode vorbereitet. Android und Linux bleiben spaetere Plattformpfade.

## Grundsatz

RK Workspace uebertraegt standardmaessig keine Dateien. Ein digitales Ding bleibt Original-Owned auf seiner Originalablage. Andere Ablagen sehen und nutzen kontrollierte Frames, aber erhalten keine freie Originaldatei, keinen Originalpfad und keine automatische Besitzuebernahme.

## MA010 Implementiert

- Windows PDF Frame Pilot: `src/Tools/RKWorkspace.WindowsPdfFramePilot/`
- DevLan Labortransport: `src/Communication/RKWorkspace.Transport.Dev/`
- Windows Owner fuer macOS-Handoff: `tools/run-windows-owner-for-mac.ps1`
- macOS Guest Compatibility Harness: `src/Tools/RKWorkspace.MacGuestCompatibilityHarness/`
- iOS/iPadOS Guest Compatibility Harness: `src/Tools/RKWorkspace.iOSGuestCompatibilityHarness/`
- PDF Renderer Abstraction: `src/Frame/RKWorkspace.Frame.Pdf/`
- Frame Cache Policy: `src/Frame/RKWorkspace.Frame.Pdf/`
- Windows Agent Dev Host: `src/Agents/RKWorkspace.Agent.Windows/`
- Configuration System: `src/Configuration/RKWorkspace.Configuration/`
- Config Tool: `src/Tools/RKWorkspace.ConfigTool/`
- Ablage Identity Store: `.rkworkspace-dev/identities/` via `tools/init-ablage-identity.ps1`

## Wichtige MA010-Skripte

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-rkwp-lan-smoke.ps1
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
.\tools\run-mac-guest-compat.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-config-tool.ps1 -SmokeTest
.\tools\run-windows-agent-dev.ps1 -SmokeTest
.\tools\init-ablage-identity.ps1 -AblageName "Windows Owner" -Platform Windows
```

## Readiness

MA010 Readiness-Dateien:

- `Docs/Readiness/MA010_ReadinessReview.md`
- `Docs/Readiness/MA010_PilotTestPlan.md`
- `Docs/Readiness/MA010_NextPlatformCodexActions.md`
- `release/MA010_READINESS_SUMMARY.md`

## Plattform-Auftraege

Windows-Codex:

```text
Baue sichtbaren Windows PDF Owner/Guest Pilot weiter aus und stabilisiere Frame UI.
```

macOS-Codex:

```text
Baue macOS Frame Guest Surface mit RKWP DevLan Client.
```

iOS/iPadOS ueber macOS/Xcode-Codex:

```text
Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
```

Android-Codex spaeter:

```text
Baue Android Frame Guest Surface mit RKWP DevTransport.
```

Linux-Codex spaeter:

```text
Baue Linux Frame Guest Surface unter Wayland/X11-Beruecksichtigung.
```

Hardware/Dongle spaeter:

```text
Definiere Dongle Hardware MVP fuer Ablage-Anker mit BLE/UWB/USB.
```

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Zeitgestempelte Exporte liegen ebenfalls unter:

```text
release/codex-context/
```

## Naechster Real-Lab-Pfad

1. Windows Pilot lokal gruen ausfuehren.
2. Windows Owner per DevLan starten.
3. macOS Guest Surface auf macOS bauen.
4. Windows Owner zu macOS Guest testen.
5. Danach iPad/iPhone Surface App ueber Xcode testen.
