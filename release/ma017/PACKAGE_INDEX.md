# MA017 Package Index

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Index ist der Einstiegspunkt fuer alle MA017 Pilot- und Handoff-Pakete.

MA017 ist kein Produkt-Release. Es ist ein kontrolliertes Pilotpaket fuer Windows Owner, macOS Guest, iOS/iPadOS Guest und UWB/Dongle-Proximity.

## Pakete

### Windows Pilot Package

Pfad:

```text
release/ma017/packages/windows-pilot/
```

Inhalt:

- Windows Owner PDF Lifecycle.
- Closed PDF Capsule.
- Open PDF Frame.
- No File Ingress.
- Security/Policy Gates.
- Glass Edge mit naechster Ablage.

### macOS Handoff Package

Pfad:

```text
release/ma017/packages/macos-handoff/
```

Inhalt:

- macOS Guest Ablage.
- Xcode-Uebergabe.
- RKWP Swift Modelle.
- No File Ingress Contract.
- Minimal Guest App Task.

### iOS/iPadOS Handoff Package

Pfad:

```text
release/ma017/packages/ios-handoff/
```

Inhalt:

- iPad/iPhone Guest Ablage.
- Xcode-Uebergabe.
- Haptik Contract.
- USB Install Runbook.
- No File Ingress Sandbox Contract.

### UWB/Dongle Package

Pfad:

```text
release/ma017/packages/uwb-dongle/
```

Inhalt:

- UWB Simulation.
- Dongle Anchor Simulation.
- EphemeralLab Privacy.
- Hardware MVP.
- Proximity-Fusion.

## Zentrale Guides

- `release/ma017/OWNER_SHORT_GUIDE.md`
- `release/ma017/handoff/macOS_FINAL_CODEX_HANDOFF.md`
- `release/ma017/handoff/iOS_XCODE_FINAL_CODEX_HANDOFF.md`
- `Docs/Readiness/MA017_PackagingCheckpoint.md`

## Gate

```powershell
.\tools\run-ma017-packaging-checkpoint.ps1 -SmokeTest
```

## Result

```text
MA017PackageIndex: READY
MA017PackagingCheckpoint: SUCCESS
RESULT: SUCCESS
```
