# MA008 Readiness Review

Status: Draft  
Datum: 2026-07-05  
Branch: `feature/ma008-rkwp-devtransport-e2e-frame`

## Zweck

Diese Review schliesst MA008 fachlich ab. Sie trennt klar zwischen lokal implementiert, simuliert, dokumentiert, vorbereitet und blockiert.

MA008 beweist noch keinen produktiven Cross-Device-Betrieb. MA008 beweist, dass das RKWP-Original-Owned-Frame-Modell lokal testbar ist, dass No File Ingress als Gate funktioniert und dass die naechsten Plattformarbeiten eindeutig beschrieben sind.

## Gate Status

| Bereich | Status | Begruendung |
| --- | --- | --- |
| RKWP DevTransport | Done | `NamedPipeDev`, Transport-Harness, Session-Verhandlung, Heartbeat, FrameUpdate, Error, Timeout, Disconnect und Diagnostics sind lokal verifiziert. |
| AblageIdentity/Trust | Done | Ablage-Identitaet, Pairing-State, Trust-Policy und Trust-Gate blockieren Unknown, Untrusted, Revoked, Denied und Pending vor Lease/Frame. |
| Windows Local E2E | Done | Windows Owner + Windows Guest laufen lokal ueber DevTransport mit echter Sample-PDF, CarryLease, FrameSession, Return und Recovery. |
| Glass Edge + PDF Frame | Done | Glass Edge startet den FrameOnly-Pfad logisch; NearestAblage, ObjectEnteringEdge, FrameSession, Guest Frame, No File Ingress und Return sind im E2E-Smoke abgedeckt. |
| Interactive Input | Done | Scroll, Zoom, Annotation, Keyboard/Pointer-Denials, Lease-/FrameSession-Bindung, Sequence und Audit-Denials sind in RKWP-Tests abgedeckt. |
| ChangeSet | Done | Annotation-ChangeSets, Accept, Reject, ForkVersion, Expired Lease, Unsupported Operation und PolicyChanged sind getestet. |
| OwnershipTransfer | Done | Ownership Transfer ist nicht Default; CopyOut/MoveOwnership laufen nur ueber Policy, Bestaetigung und MaterializationResult. |
| Windows Object Adapter | Done | PDF-Dateireferenzen, ClipboardText und vorbereitete Screenshot-/WindowSnapshot-Stubs sind als Original-Owned Quellen modelliert. |
| macOS Handoff | Partial | Handoff-Dokumente, Starter Kit und Testplan existieren. Native macOS Guest Surface und netzwerkfaehiger DevTransport fehlen noch. |
| iOS/iPadOS Handoff | Partial | Xcode-Handoff, Surface-Plan, Berechtigungen und mobile Testplaene existieren. Native iPad/iPhone App fehlt noch. |
| Android/Linux Starter | Planned | Starter Kits und Plattformnotizen existieren. Native Implementierungen sind spaetere Arbeitspakete. |
| Proximity/ManualMap | Done | Simulated Provider, Manual Map, Entfernung, Confidence, Hysterese, StableEdgeSwitch und Single-Glass-Edge-Smoke sind vorhanden. |
| Dongle Roadmap | Planned | Dongle/BLE/UWB sind als spaetere Identitaets-, Trust- und Proximity-Anker dokumentiert, aber noch nicht implementiert. |
| Audit Diagnostics | Done | JSONL-Audit-Store, Diagnostics-Tool, ReadLog und Smoke-Test fuer Events, Sessions, Recovery und No File Ingress existieren. |
| Security Gate | Done | Production blockiert DevelopmentInsecure, fehlenden Audit, fehlenden Replay-Schutz und fehlendes Policy Binding; Development/Test sind sichtbar unsicher. |

## Technisch Umgesetzt

- RKWP DevTransport fuer lokale Windows-Development-Tests.
- AblageIdentity, Pairing-State und Trust Gate als Lease-/Frame-Vorstufe.
- Windows Local Frame E2E mit Sample-PDF und No File Ingress.
- Glass Edge PDF Frame E2E als logischer Produktpfad.
- PDF Frame Interaction mit policygebundenem Input und ChangeSets.
- Ownership Transfer Guard und Materialization.
- Windows Object Adapter fuer Original-Owned Quellen.
- Diagnostics/Audit Viewer fuer Development JSONL-Logs.
- Security Gate fuer produktive Mindestregeln.

## Simuliert

- Cross-Device-Transport ausserhalb lokaler Windows-Prozesse.
- macOS/iPad/iPhone/Android/Linux Guest Surfaces.
- echte Entfernungsmessung; Manual Map und simulierte Proximity stehen bereit.
- Glass Edge Gegenkante und RemotePlacement ausserhalb des lokalen Smokes.
- PDF-Rendering auf Gastplattformen; aktuell ist die Frame-Mechanik wichtiger als Seitenrendering.

## Dokumentiert

- RKWP DevTransport.
- AblageIdentity, Pairing und Trust.
- Windows Local Frame E2E.
- Glass Edge PDF Frame E2E.
- PDF Frame Interaction.
- Windows-to-macOS Handoff.
- iOS/iPadOS Handoff.
- Diagnostics/Audit.
- Security Gate.
- MA008 Testmatrix und Next Actions.

## Blockiert

- echter Windows-to-macOS-Test ohne netzwerkfaehiges DevTransport-Profil.
- native macOS Frame Guest Surface.
- native iPad/iPhone Surface App ueber Xcode.
- produktive Verschluesselung und Mutual Authentication.
- produktiver Trust Store und Pairing UI.
- echter PDF-Renderer auf Gastplattformen.
- echte BLE/UWB/Dongle-Proximity.

## Was Owner Lokal Testen Kann

```powershell
.\tools\run-tests.ps1
.\tools\run-rkwp-tests.ps1
.\tools\run-rkwp-transport.ps1 -SmokeTest
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
```

Diese Tests pruefen Struktur, Protokoll, lokale Frame-Mechanik, No File Ingress, Diagnostics und Security Gate.

## Was macOS-Codex Braucht

- `release/codex-context/RKWorkspace_Context_latest.zip`
- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `release/handoff/WindowsToMac_MA008_Handoff.md`
- ein netzwerkfaehiges RKWP DevTransport-Profil als naechsten gemeinsamen Auftrag.

## Was iOS/Xcode Braucht

- `release/codex-context/RKWorkspace_Context_latest.zip`
- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md`
- `release/handoff/iOS_iPadOS_MA008_Handoff.md`
- native Surface App mit Local Network Permission, FrameGuestSurface und No-File-Ingress-Log.

## Naechster Echter Cross-Device-Test

Der naechste echte Cross-Device-Test ist:

1. Windows bleibt PDF Owner.
2. macOS wird als Guest Ablage gepairt.
3. DevTransport laeuft netzwerkfaehig im lokalen LAN.
4. macOS zeigt nur den Frame.
5. macOS erhaelt keine Originaldatei, keinen Originalpfad und keine Originalbytes.
6. Heartbeat, Return und Recovery sind in Logs sichtbar.

Danach folgt derselbe Pfad fuer iPad/iPhone ueber Xcode.

## Gate Entscheidung

MA008 ist bereit fuer den ersten echten Testing-Block auf Plattformgrenzen. Die lokale Struktur steht, das Protokoll steht, No File Ingress ist getestet und die Blocker sind klar benannt.

Nicht bereit ist MA008 fuer produktive Nutzung, echte Security, echten Cross-Device-Betrieb oder Hardware-/Dongle-Tests.
