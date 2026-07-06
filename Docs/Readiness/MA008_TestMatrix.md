# MA008 Test Matrix

Status: Draft  
Datum: 2026-07-05  
Branch: `feature/ma008-rkwp-devtransport-e2e-frame`

## Zweck

Diese Matrix beschreibt, welche Tests MA008 abdecken, welches Script sie ausfuehrt, welches Erfolgskriterium gilt und was der naechste Schritt ist.

| Testname | Plattform | Status | Script | Erfolgskriterium | Blocker | Naechster Schritt |
| --- | --- | --- | --- | --- | --- | --- |
| Foundation Tests | Windows/.NET | Done | `.\tools\run-tests.ps1` | Build 0 Warnungen/0 Fehler, Core, Agent, Studio, Shell, Glass Edge, RKWP und Frame-Smokes enden mit SUCCESS. | keine lokal bekannten | weiter als Pflicht-Gate vor Commit nutzen |
| RKWP Tests | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | alle Protocol-, Ownership-, Security-, Trust-, Input-, ChangeSet- und Diagnostics-Regeln PASS. | keine lokal bekannten | bei jedem Protokollwechsel erweitern |
| PDF Frame Smoke | Windows/.NET | Done | `.\tools\run-pdf-frame-smoke.ps1` | Sample-PDF, FrameOnly, OwnerLocked, NoFileIngress, Return und Recovery SUCCESS. | echter nativer PDF-Renderer fehlt | sichtbaren Frame Provider verbessern |
| No File Ingress | Windows/.NET | Done | Teil von `run-rkwp-tests`, `run-pdf-frame-smoke`, `run-windows-local-frame-e2e`, `run-glass-edge-pdf-frame-e2e` | GuestHasPdfFile NO, GuestHasOriginalPath NO, OriginalFileBytes NO. | keine lokal bekannten | als hartes Gate fuer Plattformtests beibehalten |
| RKWP DevTransport | Windows/.NET | Done | `.\tools\run-rkwp-transport.ps1 -SmokeTest` | NamedPipeDev, Hello, Capabilities, Session, Heartbeat, FrameUpdate, Error, Timeout, Disconnect, Diagnostics SUCCESS. | nur lokal; kein Cross-Device-Netzwerkprofil | TCP/WebSocket Development-Profil bauen |
| Windows Local Frame E2E | Windows/.NET | Done | `.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest` | Windows Owner/Guest, DevPairing, CarryLease, FrameSession, No File Ingress, Return und Recovery SUCCESS. | sichtbarer UI-Frame noch minimal | lokales PDF Frame UI verbessern |
| Glass Edge PDF Frame E2E | Windows/.NET | Done | `.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest` | NearestAblage, GlassEdge, ObjectPlaced, FrameSession, GuestFrame, No File Ingress, Return und Recovery SUCCESS. | echte zweite Plattform fehlt | mit macOS Guest Surface verbinden |
| InputChannel Tests | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | Scroll/Zoom/Annotation policygebunden, Denials auditiert, Lease/Frame/Sequence validiert. | kein nativer UI-Input auf Gastplattform | in macOS/iPad Surface anbinden |
| ChangeSet Tests | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | Annotation ChangeSet, Accept/Reject, ForkVersion, Expired Lease und Conflict-Regeln PASS. | keine echte PDF-Seitenannotation UI | mit echtem PDF Viewer verbinden |
| OwnershipTransfer Tests | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | Default-Deny, CopyOut/MoveOwnership nur policy- und bestaetigungsgebunden. | produktive UX fuer Ownership-Entscheidung fehlt | spaeter Owner-Settings UI bauen |
| Windows Object Adapter | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | PDF-Dateireferenz bleibt OriginalOwned; kein Guest File entsteht. | echte Desktop-Objekterkennung noch nicht produktiv | Explorer/PDF-Adapter spaeter vertiefen |
| Proximity Tests | Windows/.NET | Done | `.\tools\run-glass-edge.ps1 -SmokeTest` | ManualMap, NearestAblage, Hysterese, SingleGlassEdge und DistanceConfidence SUCCESS. | echte BLE/UWB/Dongle-Sensorik fehlt | Manual Map UI und Dongle Requirements |
| Mobile Glass Edge Smoke | Windows/Web-Prototyp | Done | `.\tools\run-mobile-glass-edge.ps1 -SmokeTest` | mobile Surface, SingleGlassEdge, Haptics und NoTechnicalWords SUCCESS. | keine native iOS/Android App | Xcode/Android Native Surface bauen |
| Diagnostics | Windows/.NET | Done | `.\tools\run-rkwp-diagnostics.ps1 -SmokeTest` | AuditLog, Events, ActiveSessions, Recovery, PolicyDenied und NoFileIngress SUCCESS. | Viewer noch CLI-basiert | Viewer fuer Cross-Device Logs erweitern |
| RKWP Performance Baseline | Windows/.NET | Done | `.\tools\run-rkwp-perf.ps1 -SmokeTest` | 10 Iterationen, JSON/Markdown-Report, FrameUpdate-Groesse, DevTransport-Latenz, Heartbeat, FrameOpen, Rueckgabe, Recovery und NoFileIngress SUCCESS. | echter Renderer und echter Cross-Device-Transport fehlen | nach macOS/iPad Surface und PDF-Renderer erneut messen |
| Security Gate | Windows/.NET | Done | `.\tools\run-rkwp-tests.ps1` | Production blockiert DevelopmentInsecure, fehlenden Audit, ReplayProtection und PolicyBinding. | echte Kryptografie/Mutual Auth fehlt | produktive Session Protection designen |
| Context Export | Windows/PowerShell | Done | `.\tools\export-codex-context.ps1` | `release/codex-context/RKWorkspace_Context_latest.zip` wird erzeugt. | keine lokal bekannten | vor Plattform-Handoff erneut exportieren |
| macOS Handoff Prepared | macOS geplant | Partial | kein lokales Windows-Script fuer echte macOS App | Docs, Starter Kit und Handoff-Dateien existieren. | native macOS Surface und Netzwerk-DevTransport fehlen | macOS-Codex mit Context Pack starten |
| iOS/iPadOS Handoff Prepared | iOS/iPadOS geplant | Partial | kein lokales Windows-Script fuer Xcode App | Xcode-Handoff, Permissions und Testplan existieren. | native App, echtes Geraet, Local Network Permission fehlen | iOS Surface App ueber macOS-Codex bauen |
| Android/Linux Starter | Android/Linux geplant | Planned | kein Smoke in MA008 | Starter Kits und Plattformnotizen existieren. | native Implementierungen fehlen | nach macOS/iOS priorisieren |

## Mindest-Gesamtverifikation

AP020 fordert die volle verfuegbare lokale Kette:

```powershell
.\tools\run-tests.ps1
.\tools\run-demo.ps1
.\tools\run-agent.ps1 -Once
.\tools\run-dual-agent.ps1
.\tools\run-local-ipc.ps1
.\tools\run-shell.ps1 -Once
.\tools\run-shell.ps1 -OverlaySmokeTest
.\tools\run-spatial-tray.ps1 -SmokeTest
.\tools\run-studio.ps1 -SmokeTest
.\tools\run-native-overlay.ps1 -SmokeTest
.\tools\run-visual-reality.ps1 -SmokeTest
.\tools\run-living-lens.ps1 -SmokeTest
.\tools\run-glass-edge.ps1 -SmokeTest
.\tools\run-mobile-glass-edge.ps1 -SmokeTest
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\export-codex-context.ps1
```

## Interpretation

Wenn diese Matrix gruen ist, haben wir Struktur, Protokoll, lokale E2E-Mechanik, No File Ingress, Trust-Regeln, Security Gate, Diagnostics und Handoff-Dokumentation.

Wenn sie gruen ist, duerfen wir in den ersten echten Testing-Block gehen. Dieser Testing-Block ist aber Development/Preview und nicht produktiv.
