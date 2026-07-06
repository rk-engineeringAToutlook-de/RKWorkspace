# Windows to iPad First Real Test Gate

Status: MA013.17 handoff  
Datum: 2026-07-06

## Auftrag

Baue und pruefe den ersten Windows-zu-iPad-Frame-Test.

## Windows

```powershell
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -SmokeTest
```

## iPad ueber macOS/Xcode

Nutze:

- `Docs/Platform/iOS_XcodeProjectBootstrap.md`
- `Docs/Platform/iOS_RKWPClientFlow.md`
- `Docs/Platform/iOS_FramePresenter.md`
- `Docs/Platform/iOS_HapticsAndGesturePrototype.md`
- `Docs/Platform/iOS_NoFileIngressSandboxChecklist.md`
- `Docs/Platform/iOS_USBDeviceTestRunbook.md`

## Erfolg

```text
FrameView: OK
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
```
