# MA016 Cross-Device Owner Instructions

## Goal

Run the Windows owner side for the first macOS and iPad PDF frame pilots.

## Before Starting

```powershell
.\tools\run-ma016-smoke.ps1 -SkipHeavy
.\tools\run-cross-device-session-monitor.ps1 -SmokeTest
```

## macOS Closed PDF

```powershell
.\tools\run-pilot-windows-to-mac-closed-pdf.ps1 -SmokeTest
```

Then continue on macOS with:

```text
release/ma016/handoff/macOS_START_HERE.md
```

## macOS Open PDF

```powershell
.\tools\run-pilot-windows-to-mac-open-pdf.ps1 -SmokeTest
```

## iPad Closed PDF

```powershell
.\tools\run-pilot-windows-to-ipad-closed-pdf.ps1 -SmokeTest
```

Then install the mobile app through Xcode:

```text
Docs/Testing/MA016_iOS_USBInstallRunbook.md
```

## iPad Open PDF

```powershell
.\tools\run-pilot-windows-to-ipad-open-pdf.ps1 -SmokeTest
```

## Success Language

Expected:

- `RESULT: SUCCESS`
- `NoFileIngress: SUCCESS`
- `FrameCapsule: OK`
- `OpenFrame: OK`

No push is performed by these scripts.
