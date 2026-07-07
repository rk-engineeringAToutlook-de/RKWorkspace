# MA017 Packaging Checkpoint

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Checkpoint schliesst AP701-710 ab.

MA017 besitzt damit ein geordnetes Pilot-Handoff-Paket fuer Windows, macOS, iOS/iPadOS und UWB/Dongle.

## Abgedeckte Arbeitspakete

- AP701: Windows Pilot Package MA017.
- AP702: macOS Handoff Package MA017.
- AP703: iOS Handoff Package MA017.
- AP704: UWB/Dongle Package MA017.
- AP705: MA017 Package Index.
- AP706: macOS Codex Uebergabe final.
- AP707: iOS/Xcode Codex Uebergabe final.
- AP708: Owner Short Guide MA017.
- AP709: Repository Hygiene MA017.
- AP710: Packaging Checkpoint MA017.

## Gate

```powershell
.\tools\run-ma017-packaging-checkpoint.ps1 -SmokeTest -AllowDirty
```

## Erwartete Marker

```text
WindowsPilotPackage: READY
macOSHandoffPackage: READY
iOSHandoffPackage: READY
UwbDonglePackage: READY
MA017PackageIndex: READY
MA017RepositoryHygiene: SUCCESS
MA017PackagingCheckpoint: SUCCESS
RESULT: SUCCESS
```
