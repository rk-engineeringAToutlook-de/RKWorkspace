# MA017 Real Test Runbook

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Runbook fuehrt den kontrollierten MA017-Owner-Test.

## Vorbereitung

1. Repository auf Branch `feature/ma016-ma017-real-platform-pilot` oeffnen.
2. Build-Artefakte bereinigen.
3. MA017 Smoke ausfuehren.
4. Owner Short Guide lesen.
5. Handoff-Paket fuer Zielplattform waehlen.

## Windows Owner Test

```powershell
.\tools\run-ma017-windows-pdf-pilot.ps1 -SmokeTest
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
```

## macOS Guest Vorbereitung

```powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
```

Danach Xcode-Handoff:

```text
release/ma017/handoff/macOS_FINAL_CODEX_HANDOFF.md
```

## iOS/iPadOS Guest Vorbereitung

```powershell
.\tools\run-ma017-ios-handoff.ps1 -SmokeTest
```

Danach Xcode-/USB-Handoff:

```text
release/ma017/handoff/iOS_XCODE_FINAL_CODEX_HANDOFF.md
```

## Proximity

```powershell
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
```

## Abschluss

```powershell
.\tools\run-ma017-smoke.ps1 -SkipHeavy
```

## Result

```text
MA017RealTestRunbook: READY
RESULT: SUCCESS
```
