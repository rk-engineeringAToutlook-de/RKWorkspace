# MA010 Readiness Summary

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma010-real-cross-device-frame-foundation`

## Commit-Stand

Abgeschlossene MA010-Implementierungscommits vor dieser Summary:

```text
e13e496 feat(frame): add windows pdf frame pilot
9423fae feat(transport): add rkwp dev lan transport
270a67d feat(platform): prepare windows owner for mac devlan
c88661c feat(platform): add mac guest compatibility harness
90776ef feat(platform): add ios guest compatibility harness
cdc30de feat(frame): add pdf frame renderer abstraction
c298022 feat(frame): add frame cache policy
01916ff feat(agent): prepare windows agent dev host
c589d36 feat(config): add rk workspace configuration system
```

Der AP040-Readiness-Commit enthaelt diese Summary selbst.

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Zeitgestempelte Exporte liegen ebenfalls unter:

```text
release/codex-context/
```

Der finale AP040-Export wird durch `.\tools\export-codex-context.ps1` erzeugt und aktualisiert den stabilen `latest`-Pfad.

## Teststatus

AP040 fuehrt vor Commit die gesamte lokal vorhandene Testkette aus:

- `run-tests.ps1`
- `run-demo.ps1`
- `run-agent.ps1 -Once`
- `run-dual-agent.ps1`
- `run-local-ipc.ps1`
- `run-shell.ps1 -Once`
- `run-shell.ps1 -OverlaySmokeTest`
- `run-spatial-tray.ps1 -SmokeTest`
- `run-studio.ps1 -SmokeTest`
- `run-native-overlay.ps1 -SmokeTest`
- `run-visual-reality.ps1 -SmokeTest`
- `run-living-lens.ps1 -SmokeTest`
- `run-glass-edge.ps1 -SmokeTest`
- `run-mobile-glass-edge.ps1 -SmokeTest`
- `run-rkwp-tests.ps1`
- `run-pdf-frame-smoke.ps1`
- `run-windows-local-frame-e2e.ps1 -SmokeTest`
- `run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest`
- `run-rkwp-diagnostics.ps1 -SmokeTest`
- `run-policy-profile.ps1 -SmokeTest`
- `run-manual-map.ps1 -SmokeTest`
- `run-rkwp-perf.ps1 -SmokeTest`
- `run-rkwp-lan-smoke.ps1`
- `run-windows-owner-for-mac.ps1 -SmokeTest`
- `run-mac-guest-compat.ps1 -SmokeTest`
- `run-ios-guest-compat.ps1 -SmokeTest`
- `run-windows-pdf-frame-pilot.ps1 -SmokeTest`
- `run-config-tool.ps1 -SmokeTest`
- `run-windows-agent-dev.ps1 -SmokeTest`
- `export-codex-context.ps1`

## Was Windows Lokal Kann

- echte PDF lokal als OriginalOwned Ding registrieren
- Windows Owner und Windows Guest simulieren
- CarryLease und FrameSession oeffnen
- Guest FrameOnly anzeigen
- Owner sperren und nach Rueckgabe freigeben
- No File Ingress pruefen
- Recovery pruefen
- DevLan im Laborprofil rauchen testen
- Config, Policy, ManualMap, Diagnostics und Performance-Smoke ausfuehren

## Was Fuer macOS Bereit Ist

- Windows Owner Handoff
- DevLan-Labortransport
- macOS Compatibility Harness
- macOS Starter Kit und Permissions
- konkreter macOS-Codex-Auftrag:

```text
Baue macOS Frame Guest Surface mit RKWP DevLan Client.
```

## Was Fuer iOS/iPadOS Bereit Ist

- iOS/iPadOS Compatibility Harness
- Xcode-Handoff
- USB-Testplan
- Haptik-/Touch-Modell
- Sandbox- und No-File-Ingress-Regeln
- konkreter iOS/iPadOS-Auftrag:

```text
Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
```

## PDF Frame Status

Die Sample-PDF ist real und bleibt beim Owner. Gastablagen sehen nur eine Frame-Repraesentation. Der Dev Renderer liefert aktuell Placeholder/MetadataPreview mit echten Metadaten; echter PDF-Seitenrenderer bleibt der naechste visuelle Qualitaetsschritt.

## No File Ingress Status

No File Ingress ist ein hartes Gate und bleibt in MA010 in mehreren Pfaden getestet:

- PDF Frame Smoke
- Windows Local Frame E2E
- Glass Edge PDF Frame E2E
- Windows PDF Frame Pilot
- Frame Cache
- RKWP Protocol Tests

## DevLan Status

DevLan ist bereit fuer Laborversuche und Handoff, aber nicht fuer Produktion. Es gibt Owner/Guest/Smoke-Scripts, Config-Samples, Heartbeat, Disconnect und FrameUpdate. Offene Punkte: finales TLS, produktive Mutual Auth, Trust Store und Pairing UI.

## Windows Pilot Status

Der Windows Pilot ist der erste reale technische Referenzpfad. Er zeigt eine echte PDF als Original-Owned FrameOnly Session mit Rueckgabe, Recovery und No File Ingress.

## Offene Blocker

- native macOS Frame Guest Surface
- native iPad/iPhone Surface App
- echter PDF-Seitenrenderer
- produktive Security
- echte Proximity-Hardware oder BLE/UWB/Dongle
- finale Owner-UX fuer explizite Besitzuebernahme
- produktive Windows Service-/Tray-Installation

## Empfehlung Fuer Den Ersten Echten Cross-Device-Test

1. Windows Pilot lokal erneut gruen ausfuehren.
2. Windows Owner mit DevLan starten.
3. macOS-Codex baut macOS Frame Guest Surface.
4. Windows Owner zu macOS Guest testen.
5. Danach iPad/iPhone ueber Xcode anbinden.

## Wichtigste Aussage

Windows kann lokal einen echten PDF-Frame als Original-Owned Session testen. macOS ist konkret handoff-bereit. iOS/iPadOS ist konkret Xcode-handoff-bereit. RKWP hat DevTransport, Identity, Security-Struktur, Policy, Frame, Lease, Recovery und No File Ingress. Der naechste Schritt ist der erste echte Windows-zu-macOS-oder-iPad-Test.
