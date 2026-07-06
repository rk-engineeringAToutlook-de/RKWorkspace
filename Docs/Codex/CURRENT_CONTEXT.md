# Current Codex Context

Datum: 2026-07-06  
Branch: `feature/ma013-real-cross-platform-frame-pilot`

## Aktueller Stand

MA013 bereitet den ersten echten Cross-Device-Pilot vor. Windows ist die aktive Owner-Referenz. macOS ist der naechste Guest-Pfad. iPad/iPhone folgt danach ueber Xcode/TestFlight. RK Workspace bleibt beim Original-Owned Frame Modell: Die Originaldatei bleibt auf der Owner-Ablage, andere Ablagen sehen kontrollierte Frames.

## Grundsatz

RK Workspace uebertraegt standardmaessig keine Dateien. Ein digitales Ding bleibt Original-Owned auf seiner Originalablage. Andere Ablagen sehen und nutzen kontrollierte Frames, erhalten aber keine freie Originaldatei, keinen Originalpfad und keine automatische Besitzuebernahme.

## MA013 Implementiert

- macOS Repository Bootstrap, Client Flow, Contract und Guest Surface Readiness.
- iOS/iPadOS Xcode, USB, Haptics, Return und No File Ingress Readiness.
- SecureDev, Security Gate, Policy Profile, Audit, Recovery und Diagnostics Readiness.
- PDF Frame Renderer, Multi-Page, Tiles, Annotation ChangeSets, Text Extraction Policy und Renderer Readiness.
- Windows Object Adapter Readiness fuer PDF, Explorer, Clipboard, Screenshot, WindowSnapshot und RemoteSession.
- Proximity/Distance Readiness fuer Manual Map, BLE, WiFi, UWB, Dongle, USB Control und Sensor Fusion.
- UX/HX Readiness fuer Glass Edge, digitale Hand, Owner/Guest-Zustaende, Haptics und Fehler-UX.
- Operations Readiness fuer Config, Policy, Diagnostics, Audit Retention, Offline/Reconnect und Identity Backup/Restore.
- Install Readiness fuer Windows Dev Package, macOS/iOS/Android/Linux Plaene, CI, Signing und Updates.
- Performance Readiness fuer E2E-Baseline, Multi-Frame-Load, Latenz, Memory, Mobile Battery, Network Adaptation und Compression.

## Wichtige Skripte

```powershell
.\tools\run-tests.ps1
.\tools\run-rkwp-tests.ps1
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\run-rkwp-load.ps1 -SmokeTest
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-windows-securedev-pdf-e2e.ps1 -SmokeTest
.\tools\run-rkwp-securedev-smoke.ps1
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
.\tools\run-manual-map.ps1 -SmokeTest
.\tools\run-windows-agent-dev.ps1 -SmokeTest
.\tools\package-windows-dev.ps1 -SmokeTest
.\tools\export-rkwp-schema.ps1
.\tools\export-codex-context.ps1
```

## Readiness

MA013 Readiness-Dateien:

- `Docs/Readiness/MA013_ReadinessReview.md`
- `Docs/Readiness/MA013_RealPilotStartPlan.md`
- `Docs/Readiness/MA013_NextCodexActions.md`
- `Docs/Readiness/PilotAcceptanceCriteria.md`
- `release/MA013_READINESS_SUMMARY.md`

## Plattform-Auftraege

Windows-Codex:

```text
Starte Windows als Owner-Referenz, fuehre echte PDF als Original-Owned Frame und halte No File Ingress, Return, Recovery, Audit und Glass Edge stabil.
```

macOS-Codex:

```text
Baue eine native macOS Frame Guest Surface mit AblageIdentity, RKWP DevLan/SecureDev Client, FrameSession-Anzeige, Heartbeat, Return und No File Ingress.
```

iOS/iPadOS ueber macOS/Xcode-Codex:

```text
Baue eine iPad/iPhone RK Workspace Surface App per Xcode, die einen RKWP Frame anzeigen kann, Haptik vorbereitet, Return senden kann und keine PDF-Datei speichert.
```

Android-Codex spaeter:

```text
Baue Android Frame Guest Surface mit RKWP DevTransport, nachdem Windows/macOS/iPad verifiziert sind.
```

Linux-Codex spaeter:

```text
Baue Linux Frame Guest Surface unter Wayland/X11-Beruecksichtigung, nachdem der erste echte Cross-Device-Test stabil ist.
```

Hardware/Dongle spaeter:

```text
Definiere Dongle Hardware MVP fuer Ablage-Anker, Distanzsignal und Trust-Hilfe nach dem ersten echten Windows-to-macOS-Test.
```

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

## Naechster Real-Lab-Pfad

1. Windows Pilot lokal gruen ausfuehren.
2. Context Pack erzeugen.
3. macOS-Codex liest Context Pack.
4. Windows Owner mit DevLan/SecureDev starten.
5. macOS Guest Surface bauen und starten.
6. `AblageHello`, FrameSession, Heartbeat, Return und Recovery testen.
7. No File Ingress bestaetigen.
8. Danach iPad/iPhone Surface App ueber Xcode testen.
