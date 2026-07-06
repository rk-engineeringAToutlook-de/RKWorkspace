# MA010 Readiness Review

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma010-real-cross-device-frame-foundation`

## Zweck

MA010 schliesst die Real Cross-Device Foundation ab. Die lokale Windows-Strecke ist nun stark genug, um echte PDF-Frame-Tests als Original-Owned Session zu fahren. macOS und iOS/iPadOS sind noch nicht produktiv implementiert, aber als naechste Plattform-Codex-Auftraege konkret vorbereitet.

Der Standard bleibt:

- keine freie Datei auf der Gastablage
- keine automatische Besitzuebernahme
- FrameOnly statt Dateitransfer
- Rueckgabe und Recovery als Pflichtpfade
- immer genau eine naechste Ablage fuer die sichtbare Glass Edge

## Statusmatrix

| Bereich | Status | Begruendung |
| --- | --- | --- |
| Secure Session Spike | Partial | Dev-Zertifikate, Dev-Identity, Nonce/Sequence, Replay-Schutz, Policy Binding und Audit sind lokal getestet. Produktive Kryptografie, echter Trust Store und finale Mutual Auth bleiben offen. |
| DevLan Transport | Partial | DevLan ist als Labortransport mit Owner/Guest-Scripts, LAN-Smoke, DevPairing, Heartbeat, FrameUpdate und Config-Samples vorbereitet. Finales TLS und echte produktive Security fehlen. |
| Windows Owner for macOS | Done | `run-windows-owner-for-mac.ps1 -SmokeTest` startet den Windows Owner als macOS-Handoff-Pfad, zeigt AblageId, DevLan-URL, PDF, Lease-Pfad, No-File-Ingress-Pflicht und sauberen Timeout/Shutdown. |
| macOS Handoff | Partial | macOS-Codex-Auftrag, Compatibility Harness, Starter Kit, Permissions und Handoff-Doku existieren. Native macOS Frame Guest Surface muss auf macOS/Xcode gebaut werden. |
| iOS Handoff | Partial | iOS/iPadOS Harness, Xcode-Handoff, USB-Testplan, Haptik-/Gestenhinweise und No-File-Ingress-Regeln existieren. Native iPad/iPhone App muss ueber macOS-Codex und Xcode entstehen. |
| Manual Map | Done | ManualMap-Tool und Glass Edge E2E koennen Ablagen, Richtung, Entfernung, Hysterese und naechste Ablage fuer den Laborraum simulieren. Lokale echte Config bleibt unversioniert. |
| PDF Renderer | Partial | `IPdfFrameRenderer`, RenderRequest/Result, Renderer-Diagnostik, Capabilities und Poppler Development Renderer existieren. Die erste Seite wird als PNG-Frame gerendert, wenn Poppler verfuegbar ist. Produktives PDFium/MuPDF-Packaging bleibt offen. |
| Frame Cache | Done | MemoryOnly/DevInspectable/TemporaryEncrypted/Disabled sind modelliert. FrameClose, Revocation und Recovery loeschen Cache; Original-PDF, Originalbytes und Originalpfad werden nicht gespeichert. |
| Windows Agent Plan | Partial | Windows Agent Dev Host, Install-/Uninstall-Smokes und Plattformdokumente existieren. Produktive Service-Installation, Tray und Autostart sind bewusst spaeter. |
| Configuration System | Done | Einheitliche Config-Modelle, Sample-JSONs, Validator, ConfigTool und Gitignore-Regeln existieren. Lokale Secrets werden nicht erwartet und nicht versioniert. |
| PDF Frame Pilot | Done | Windows PDF Frame Pilot nutzt echte Sample-PDF, Owner/Guest-Zustaende, CarryLease, FrameSession, Return, Recovery und No File Ingress. Ab MA011.04 kann der Development-Pfad die erste Seite als PNG-Frame rendern. |
| No File Ingress | Done | RKWP-Tests, PDF Frame Smoke, Windows Local E2E, Glass Edge E2E, Pilot und Cache-Regeln pruefen: keine Guest-Datei, kein Originalpfad, keine Originalbytes. |
| Policy Profiles | Done | CriticalInfrastructure, OfficeDefault, DevelopmentLab, PresentationOnly und TrustedPersonalDevices sind implementiert, getestet und in Config validierbar. |
| Audit | Done | RKWP Diagnostics schreibt lokale JSONL-Auditlogs und zaehlt Sessions, Leases, FrameSessions, Heartbeats, PolicyDenied, Recovery und SecurityViolations. |
| Glass Edge Integration | Partial | Single Glass Edge, Nearest Ablage, Glass Edge PDF Frame E2E und mobile Smoke existieren. Finale native Produktoberflaeche und echte Proximity-Hardware sind offen. |

## Was Windows Lokal Kann

- echte PDF als OriginalThing registrieren
- lokale Owner- und Guest-Ablage simulieren
- Original-Owned CarryLease und FrameSession oeffnen
- Guest FrameOnly ohne PDF-Datei anzeigen
- Owner sichtbar sperren und nach Rueckgabe freigeben
- Recovery nach Heartbeat-/Lease-Verlust pruefen
- DevLan und NamedPipeDev lokal rauchen testen
- Config, Policy, ManualMap, Diagnostics und Performance-Smoke ausfuehren

## Was Fuer macOS Bereit Ist

- Windows Owner Startbefehl und Handoff-Dateien
- DevLan-Labortransport als Zielprofil
- macOS Guest Compatibility Harness als RKWP-Verhaltensspezifikation
- macOS Starter Kit, Permission Notes und Build Notes
- klare No-File-Ingress-Kriterien

Blocker: native macOS Surface und echter macOS DevLan Client muessen auf macOS gebaut werden.

## Was Fuer iOS/iPadOS Bereit Ist

- iOS/iPadOS Guest Compatibility Harness
- Xcode-Handoff-Auftrag fuer macOS-Codex
- USB-Testplan fuer echtes iPad/iPhone
- Haptik-/Gestenmodell und Sandbox-Grenzen
- No File Ingress als harte Regel

Blocker: iPhone und iPad haben keinen eigenen Codex. Die native App muss ueber macOS-Codex und Xcode entstehen.

## Android Und Linux

Android und Linux bleiben vorbereitet, aber nicht Hauptpfad fuer den ersten Real-Lab-Test. Starter-Kits existieren; echte Implementierungen folgen nach Windows/macOS/iPad-Stabilisierung.

## Produktionsnaehe

Produktionsnah sind die semantischen RKWP-Regeln:

- Original-Owned
- FrameOnly
- No File Ingress
- Policy Binding
- Audit
- Rueckgabe
- Recovery

Noch Dev/Lab sind:

- DevLan ohne finales TLS
- Dev-Zertifikate
- produktives PDF-Renderer-Packaging und Security-Update-Pfad
- lokale ManualMap statt echter Distanzmessung
- Windows Agent ohne produktive Installation
- simulierte macOS/iOS Guest Harnesses

## Gate-Entscheidung

MA010 ist bereit fuer den ersten echten Cross-Device-Versuch im Labor:

1. Windows lokal als Referenz gruen ausfuehren.
2. Windows Owner mit DevLan starten.
3. macOS Guest Surface ueber macOS-Codex bauen.
4. Windows Owner zu macOS Guest testen.
5. Danach iPad/iPhone ueber Xcode anbinden.

Noch nicht bereit ist MA010/MA011 fuer produktive Nutzung, kritische Umgebungen, echte Besitzuebernahme ohne Owner-UX, finale Security, echte Hardware-Proximity oder vollstaendiges PDF-Rendering auf jeder Plattform. Der Poppler Development Renderer ist ein echter Preview-Slice, aber noch keine Produktentscheidung.
