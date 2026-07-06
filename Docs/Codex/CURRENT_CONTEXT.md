# Current Codex Context

Datum: 2026-07-06  
Branch: `feature/ma011-secure-real-frame-cross-device-foundation`

## Aktueller Stand

MA011 bereitet den ersten echten Windows-to-macOS-Test vor. Windows ist der aktive technische Referenzpfad und kann lokal mit einer echten PDF als Original-Owned Frame arbeiten. macOS hat nun einen konkreten Build- und Handoff-Plan. iOS/iPadOS hat einen konkreten Xcode- und USB-Testplan.

## Grundsatz

RK Workspace uebertraegt standardmaessig keine Dateien. Ein digitales Ding bleibt Original-Owned auf seiner Originalablage. Andere Ablagen sehen und nutzen kontrollierte Frames, erhalten aber keine freie Originaldatei, keinen Originalpfad und keine automatische Besitzuebernahme.

## MA011 Implementiert

- produktiver Secure-Session-Pfad als Architektur- und Testbasis.
- lokaler Ablage Identity Store.
- SecureDevTransport Spike mit DevelopmentAuthenticated Fallback.
- PDF First Page Frame Rendering im Development-Pfad.
- stabilisierter Windows PDF Frame Pilot.
- Glass Edge Startpfad fuer den PDF Pilot.
- erweiterte Manual Ablage Map CLI.
- konkretes macOS Build-Layout und Handoff.
- konkreter iOS/iPadOS Xcode- und USB-Testplan.
- MA011 Readiness Review und First-Real-Test-Dokumente.

## Wichtige MA011-Skripte

```powershell
.\tools\run-tests.ps1
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -UseGlassEdge -PlaySequence -SmokeTest
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
.\tools\run-mac-guest-compat.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-manual-map.ps1 -SmokeTest
.\tools\export-codex-context.ps1
```

## Readiness

MA011 Readiness-Dateien:

- `Docs/Readiness/MA011_ReadinessReview.md`
- `Docs/Readiness/MA011_WindowsToMac_FirstRealTest.md`
- `Docs/Readiness/MA011_iPad_FirstRealTest.md`
- `Docs/Readiness/MA011_NextCodexActions.md`
- `release/MA011_READINESS_SUMMARY.md`

## Plattform-Auftraege

Windows-Codex:

```text
Starte Windows Owner fuer den ersten echten Windows-to-macOS-Test, stabilisiere den sichtbaren PDF Frame Pilot und halte No File Ingress als Gate.
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

Zeitgestempelte Exporte liegen ebenfalls unter:

```text
release/codex-context/
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
