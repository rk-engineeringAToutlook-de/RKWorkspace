# MA009 Readiness Review

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma009-secure-cross-device-frame-foundation`

## Zweck

MA009 schliesst die Secure Cross-Device Frame Foundation ab. Diese Review trennt klar:

- was lokal technisch real ist
- was simuliert ist
- was vorbereitet ist
- was fuer echte Plattformtests noch blockiert

RK Workspace bleibt im Standard Original-Owned und FrameOnly. Eine Gastablage sieht ein Ding im kontrollierten Frame, bekommt aber keine freie Datei und keinen automatischen Besitz.

## Statusmatrix

| Bereich | Status | Begruendung |
| --- | --- | --- |
| RKWP Core | Done | Protocol Foundation, Versioning, Envelope, Message Validation, Ownership, FrameSession, ChangeSet, Diagnostics und Policy-Bindung sind lokal getestet. |
| Secure Session Spike | Partial | Dev-Zertifikate, Ablage-Identitaet, Mutual Dev Authentication, Replay-Schutz und Policy-Bindung sind strukturell implementiert. Produktive Kryptografie/TLS ist noch offen. |
| DevTransport | Done | NamedPipeDev-Smokes pruefen Hello, Capabilities, Heartbeat, FrameUpdate, Error, Timeout, Disconnect und Diagnostics. Netzwerkfaehiger DevTransport fehlt noch. |
| Ablage Identity / Trust | Done | Trust-Level, Pairing-State, DevPairing und TrustGate blockieren Unknown, Untrusted, Revoked, Denied und Pending vor Lease/Frame. |
| PDF Frame | Done | Echte Sample-PDF wird ownerseitig geladen, gehasht und als MetadataPreview-Frame dargestellt. Echter Seitenrenderer bleibt blockiert. |
| No File Ingress | Done | PDF Frame, Windows Local E2E, Glass Edge E2E und RKWP-Tests pruefen: keine Guest-Datei, kein Originalpfad, keine Originalbytes. |
| Windows Local E2E | Done | Windows Owner + Windows Guest laufen lokal ueber DevTransport mit CarryLease, FrameSession, Return und Recovery. |
| Glass Edge PDF E2E | Done | ManualMap/Simulated Proximity waehlt genau eine Ablage; Glass Edge erzeugt CarryLease und FrameSession. |
| Input Channel | Done | Scroll, Zoom, Annotation und Denials sind policy-, lease-, frame- und sequencegebunden getestet. |
| ChangeSet | Done | Annotation ChangeSets, Accept/Reject, ForkVersion, Expired Lease und PolicyChanged sind getestet. |
| OwnershipTransfer | Partial | Default-Deny, CopyOut/MoveOwnership nur mit Policy und Bestaetigung sind modelliert. Finale Owner-UX und echte Materialisierung fehlen. |
| Windows Object Adapter | Done | PDF-Dateireferenz, ClipboardText und vorbereitete Screenshot-/WindowSnapshot-Stubs bleiben Original-Owned. |
| macOS Handoff | Partial | macOS-Codex-Auftrag, Starter Kit, Permissions und Build Notes existieren. Native macOS Frame Guest Surface fehlt. |
| iOS/iPadOS Handoff | Partial | Xcode-Handoff, USB-Testplan, Haptik-/Gestenplan und Sandbox-Hinweise existieren. Native App fehlt. |
| Android/Linux Starter | Planned | Plattformnotizen existieren aus vorherigen Paketen; native Implementierung ist spaeter. |
| Manual Map | Done | `run-manual-map.ps1` verwaltet lokale Ablage-Richtungen und Entfernungen; `config/manual-ablage-map.json` bleibt unversioniert. |
| Dongle Roadmap | Planned | Dongle/BLE/UWB bleiben Requirements- und Hardwarearbeit, nicht MA009-Implementierung. |
| Policy Profiles | Done | CriticalInfrastructure, OfficeDefault, DevelopmentLab, PresentationOnly und TrustedPersonalDevices sind implementiert und getestet. |
| Diagnostics | Done | Audit JSONL, RKWP Diagnostics und Session-Summary sind lokal testbar. |
| Performance Baseline | Done | `run-rkwp-perf.ps1 -SmokeTest` misst 10 lokale Iterationen und schreibt JSON/Markdown nach `logs/perf/`. |

## Technisch Real

- Lokale RKWP-Tests fuer Protocol, Security Gate, Secure Session Spike, Trust, Lease, Frame, Input, ChangeSet und OwnershipTransfer.
- Lokaler NamedPipeDev Transport.
- Lokaler Windows Owner/Guest Frame E2E mit echter Sample-PDF.
- Glass Edge PDF Frame E2E als logischer Pfad.
- Manual Map als nutzbare Proximity-Quelle.
- Policy Profiles als klare Umgebungsvorgaben.
- Performance-Harness mit lokalen Reports.

## Simuliert

- Cross-Device-Verbindung ausserhalb eines lokalen Windows-Prozesses.
- macOS/iOS/iPadOS/Android/Linux echte Gastoberflaechen.
- echte Entfernungsmessung per BLE/UWB/Dongle.
- echter PDF-Seitenrenderer.
- Glass Edge als finale native Produktoberflaeche.

## Vorbereitet

- Windows-to-macOS Handoff.
- macOS Frame Guest Surface Auftrag.
- iOS/iPadOS Xcode Surface App Auftrag.
- ManualMap + Glass Edge + PDF Frame Zusammenfuehrung.
- Policy Profiles fuer kritische, Buero-, Labor- und private Ablagen.
- Performance-Baseline fuer spaetere Renderer-/Transportvergleiche.

## Blockiert

- Produktive Kryptografie und echte Mutual Authentication.
- Netzwerkfaehiges Development-Transportprofil fuer Windows zu macOS.
- Native macOS Frame Guest Surface.
- Native iPad/iPhone Surface App via Xcode.
- echter PDF-Seitenrenderer ohne No-File-Ingress-Verletzung.
- echte BLE/UWB/Dongle-Proximity.
- produktive Owner-Entscheidungs-UX fuer CopyOut/MoveOwnership.

## Owner-Teststand

Der Owner kann auf Windows lokal testen:

```powershell
.\tools\run-tests.ps1
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
.\tools\run-policy-profile.ps1 -SmokeTest
.\tools\run-manual-map.ps1 -SmokeTest
.\tools\run-rkwp-perf.ps1 -SmokeTest
```

## Security Status

Development ist klar von Production getrennt. `DevelopmentInsecure` ist fuer lokale Tests erlaubt, aber Production blockiert unsichere Sessions. Dev-Zertifikate und Dev-Keys sind strukturell vorhanden, aber keine finale Produktkryptografie.

## PDF Frame Status

Die Sample-PDF bleibt beim Owner. Die Gastablage sieht nur eine Frame-Repraesentation. RendererStatus bleibt `RendererBlocked`, bis ein echter Renderer gewaehlt und No File Ingress weiterhin nachgewiesen ist.

## No File Ingress Status

No File Ingress ist in MA009 ein hartes Gate und zuletzt erfolgreich in mehreren Pfaden getestet:

- PDF Frame Smoke
- Windows Local Frame E2E
- Glass Edge PDF Frame E2E
- RKWP Protocol Tests
- Performance Baseline

## Gate-Entscheidung

MA009 ist bereit fuer den ersten echten Real-Lab-Vorbereitungsschritt. Der naechste Schritt ist nicht mehr eine weitere lokale Simulation, sondern ein echter Windows Owner plus eine zweite echte Gastplattform.

Noch nicht bereit ist MA009 fuer produktive Nutzung, echte Security, produktives Cross-Device-Netzwerk, native mobile Nutzung oder Hardware-/Dongle-Tests.
