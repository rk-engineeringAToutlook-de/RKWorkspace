# MA007 Readiness Summary

Status: Draft  
Datum: 2026-07-05  
Branch: `feature/ma007-followup-original-owned-frame-platforms`

## Commit-Stand

Letzter abgeschlossener Implementierungscommit vor dieser Summary:

```text
0cdf84875a3e4af3f5e1de9ec9d9e5207bd6993b
```

Der Readiness-Commit wird diese Summary selbst enthalten.

## Tests

AP009 zuletzt erfolgreich:

- `.\tools\run-rkwp-tests.ps1`
- `.\tools\run-glass-edge.ps1 -SmokeTest`
- `.\tools\run-tests.ps1`

AP010 fuehrt vor Commit die volle verfuegbare Testkette erneut aus.

## RKWP Status

RKWP ist als Original-Owned Frame Protocol arbeitsfaehig:

- Protocol Foundation vorhanden.
- FrameOnly-Sessions vorhanden.
- Input Channel vorhanden.
- ChangeSet/Return vorhanden.
- Ownership Transfer Materialization modelliert.
- Security-Hardening fuer Development vorhanden.

Produktiv fehlen noch Pairing, Trust Store und echte Kryptografie.

## PDF Frame Status

PDF FrameOnly ist als Smoke und Sample-Pfad vorhanden. Ein echtes plattformnatives PDF-Rendering auf macOS/iPad/Android ist noch nicht gebaut.

## No File Ingress Status

No File Ingress ist ein harter Gate-Punkt und zuletzt erfolgreich getestet. Guest-Surfaces duerfen keine Originaldatei, keinen Originalpfad und keine Originalbytes im FrameOnly-Pfad erhalten.

## Surface Status

- Windows: primaerer Testpfad, Object Adapter und Overlay-/Glass-Spikes vorhanden.
- macOS: Starter Kit vorhanden, native Guest Surface noch zu bauen.
- iOS/iPadOS: Xcode-Handoff und Starter Kit vorhanden, native Surface App noch zu bauen.
- Android: Starter Kit vorhanden, spaeter.
- Linux: Starter Kit vorhanden, spaeter.

## Proximity Status

Simulated Provider und Manual Map Provider sind vorhanden. Der Selector waehlt stabil genau eine naechste Ablage. Dongle/BLE/UWB sind Roadmap, nicht implementiert.

## Was Owner Testen Kann

- Lokale Testkette.
- RKWP Protocol Smoke.
- PDF Frame Smoke.
- Glass Edge Nearest Ablage Smoke.
- Developer/Visual-Spikes nur als Labor, nicht als Produkt.

## Was Owner Noch Nicht Testen Soll

- Produktive Cross-Device-Dateinutzung.
- macOS/iPad echte FrameGuestSurface.
- Dongle/BLE/UWB-Proximity.
- Produktive Security/Pairing.
- Produktiven PDF-Renderer auf Gastplattformen.

## Naechste Plattform

Empfohlen:

1. Windows Local End-to-End: PDF Owner + lokale Guest Surface sichtbar.
2. macOS Guest Surface als erste echte Plattformgrenze.
3. iPad Surface App ueber Xcode.

## Wichtige Dateien Fuer macOS-Codex

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`

## Context Pack Pfad

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Zeitstempel-ZIPs liegen im selben Ordner.

## Offene Punkte

- Produktive Security.
- Echter Transport.
- Native macOS Guest Surface.
- Native iPad/iPhone Surface App.
- Echter PDF-Renderer auf Guest.
- Hardware-/Dongle-Anforderungen.

## Empfehlung Fuer Den Naechsten Realen Test

Baue zuerst Windows PDF Owner + Windows Local Guest Surface End-to-End. Erst wenn der Frame lokal sichtbar und No File Ingress weiterhin PASS ist, lohnt der Sprung auf macOS und iPad.
