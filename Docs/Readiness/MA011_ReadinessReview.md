# MA011 Readiness Review

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma011-secure-real-frame-cross-device-foundation`

## Zweck

MA011 schliesst die lokale Windows-Referenz fuer Original-Owned PDF Frames weiter ab und bereitet den ersten echten Windows-to-macOS-Test vor. macOS und iOS/iPadOS sind nicht mehr nur grob geplant, sondern mit konkreten Build-Layouts, Handoff-Dateien und Testplaenen fuer den naechsten Codex vorbereitet.

Der Standard bleibt:

- Windows bleibt Owner der echten PDF.
- Gastablagen sehen nur Frames.
- keine freie Guest-Datei.
- kein Originalpfad auf Guest.
- keine Originalbytes als Datei.
- Return und Recovery bleiben Pflicht.

## Statusmatrix

| Bereich | Status | Begruendung |
| --- | --- | --- |
| Secure Session Path | Partial | Security-Gates, Replay-/Policy-Bindung, Audit und Dev-Modi sind vorhanden. Produktive Kryptografie, finaler Trust Store und echtes TLS sind noch offen. |
| Identity Store | Done | `AblageIdentityStore` kann lokale Dev-Identitaeten erzeugen/laden; Dateien sind aus Git und Context ausgeschlossen. |
| SecureDevTransport | Partial | SecureDev ueber NamedPipeDev-Fallback ist getestet, klar als DevelopmentAuthenticated markiert und mit Trust/Revocation gekoppelt. Produktiver Transport fehlt noch. |
| PDF Renderer | Partial | Poppler Development Renderer rendert echte erste Seiten als Frame, No File Ingress bleibt gewahrt. Finale Renderer- und Packaging-Entscheidung ist offen. |
| Windows PDF Pilot | Done | Pilot nutzt echte PDF, Owner/Guest-Zustaende, FrameOnly, Return, Recovery, Debug und Glass-Edge-Flow. |
| Glass Edge PDF Pilot | Done | Glass Edge kann den PDF-Pilot-Flow starten, Eventkette protokollieren und Nearest-Ablage/ManualMap nutzen. |
| Manual Map | Done | CLI unterstuetzt List/Show/Set/Remove/Clear/Import/Export/Validate/SmokeTest; Sample enthaelt mehrere Plattformen und Distanzen. |
| macOS Handoff | Done | macOS Build-Layout, RKWP-Flow, UI, Permissions, Build Commands und Testplan liegen vor; native App muss auf macOS gebaut werden. |
| iOS Handoff | Done | Xcode Layout, RKWP-Flow, Frame UI, Haptik, Gesten, USB-Testplan und Sandbox-Quellen liegen vor; native App muss ueber Xcode entstehen. |
| Surface Contracts | Done | Surface-Abstractions, Guest Compatibility Harnesses und Plattform-Docs beschreiben den mobilen/macOS Vertrag. |
| No File Ingress | Done | Protocol Tests, PDF Smoke, Windows Local E2E, Glass Edge E2E, Pilot und Cache pruefen GuestHasPdfFile/OriginalPath/OriginalBytes. |
| Lease Recovery | Done | Heartbeat, Return, Recovery und Revocation sind lokal getestet und in sichtbarer Sprache dokumentiert. |
| Context Pack | Done | `export-codex-context.ps1` enthaelt MA011-Handoff-Dateien fuer macOS/iOS und aktualisiert `RKWorkspace_Context_latest.zip`. |
| Policy | Done | Policy-Profile, Policy Binding, Deny/Confirm-Regeln und Critical/Development/Profile-Smokes sind vorhanden. |
| Audit | Done | RKWP Diagnostics schreibt Auditlog und zaehlt Sessions, Leases, FrameSessions, Heartbeats, PolicyDenied und Recovery. |
| Proximity | Partial | ManualMap, NearestSelector, Distance, Hysterese und Glass Edge sind lokal simuliert. Echte BLE/UWB/Distanzmessung fehlt. |
| Dongle | Planned | Hardware-MVP ist noch nicht definiert; nach erstem echten Cross-Device-Test priorisieren. |

## Was Windows Lokal Kann

- echte PDF als Original-Owned Ding registrieren.
- PDF-Seitenframe im Development Renderer erzeugen.
- Owner sichtbar sperren und nach Return freigeben.
- Guest ohne PDF-Datei, Originalpfad oder Originalbytes betreiben.
- Glass Edge als naechste Ablage verwenden.
- ManualMap-Distanzen fuer Testlabor pflegen.
- SecureDev/DevLan/NamedPipeDev lokal testen.
- Policy, Audit, Recovery und Performance-Smoke ausfuehren.

## Was macOS Als Naechstes Braucht

- native Swift/Xcode oder minimaler macOS Host.
- AblageIdentity laden/erzeugen.
- RKWP DevLan/SecureDev Client verbinden.
- AblageHello senden.
- FrameSession empfangen.
- PDF-Frame anzeigen.
- No File Ingress beweisen.
- Return und Heartbeat senden.

## Was iOS/iPadOS Als Naechstes Braucht

- native SwiftUI/Xcode App.
- echter iPad/iPhone USB-Test.
- Local Network Permission pruefen.
- Haptik bei FrameReady und Return.
- einfache Touch-Geste fuer Return.
- No File Ingress Probe.

## Gate-Entscheidung

MA011 ist bereit fuer den ersten echten Windows-to-macOS-Test, aber noch nicht fuer Produktivbetrieb.

Bereit:

- lokaler Windows Owner/PDF/Frame Pfad.
- macOS Handoff mit Build-Layout.
- iOS/iPadOS Xcode-Testplan.
- Context Pack fuer naechste Codex-Instanz.

Nicht bereit:

- produktive Kryptografie/TLS.
- native macOS/iOS Implementierung.
- echte Proximity-Hardware.
- finaler PDF Renderer fuer alle Plattformen.
- Dongle MVP.

## Wichtigste Aussage

Windows kann lokal mit einer echten PDF als Original-Owned Frame arbeiten. macOS hat einen konkreten Build- und Handoff-Plan. iPad/iPhone hat einen konkreten Xcode-Testplan. Der erste echte Windows-to-macOS-Test ist vorbereitet.
