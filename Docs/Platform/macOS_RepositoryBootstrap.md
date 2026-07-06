# macOS Repository Bootstrap

Status: MA013.01 baseline  
Datum: 2026-07-06  
Zielplattform: macOS Guest Surface

## Zweck

Dieses Dokument ist der Startpunkt fuer macOS-Codex. macOS wird die erste echte Gegenplattform zum Windows Owner. Der erste macOS-Test ist keine Dateiuebertragung, sondern eine Frame Guest Surface.

## Repository Klonen Oder Aktualisieren

```bash
git clone git@github.com:rk-engineeringAToutlook-de/RKWorkspace.git
cd RKWorkspace
git fetch --all --tags
git checkout feature/ma013-real-cross-platform-frame-pilot
git pull --ff-only
```

Falls der Branch noch nicht auf Remote existiert, soll macOS-Codex den vom Owner bereitgestellten Branch verwenden oder lokal vom aktuellen `main`/Feature-Stand starten.

## Context Pack Lesen

Stabiler Pfad im Repository:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

macOS-Codex liest zuerst:

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Codex/PlatformTasks/macOS.md`
- `release/handoff/macOS_Codex_MA013_Bootstrap.md`
- `release/schema/rkwp-envelope-schema-v0.1.json`

## RKWP Protocol Lesen

Pflichtdokumente:

- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`

Code-Referenz:

```text
src/Protocol/RKWorkspace.Protocol/
```

## Surface Abstractions Lesen

```text
src/Surfaces/RKWorkspace.Surface.Abstractions/
src/Surfaces/RKWorkspace.Surface.macOS/
```

macOS muss mindestens die Rolle `FrameGuestSurface` abbilden.

## Technologieoptionen

### Option A: Native Swift/Xcode

Bevorzugt fuer den ersten echten Test:

- SwiftUI/AppKit Minimal-App.
- App Sandbox mit Network Client.
- lokale AblageIdentity.
- RKWP DevLan/SecureDev Client.
- Frame-Anzeige.
- Heartbeat und Return.

### Option B: .NET MAUI/Avalonia

Nur nutzen, wenn macOS-Codex damit schneller einen stabilen Minimal-Host bauen kann. Die Sicherheits- und No-File-Ingress-Regeln bleiben unveraendert.

## Erste Testrolle

```text
Windows = Owner
macOS   = Guest Ablage
```

Windows bleibt Besitzer der echten PDF. macOS zeigt nur einen Frame.

## Pflicht-Gates

- `AblageHello: OK`
- `FrameSession: Active`
- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `OriginalFileBytes: NO`
- `NoFileIngress: SUCCESS`
- `Return: SUCCESS`

## Bekannte Blocker

- nativer macOS RKWP Client fehlt noch.
- produktive TLS/mTLS fehlt noch.
- finale macOS PDF-/Frame-Renderer-Strategie offen.
- macOS Permissions und Signing muessen lokal bestaetigt werden.
