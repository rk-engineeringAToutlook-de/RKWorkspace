# iOS MA013 Readiness Gate

Status: MA013.20 readiness  
Datum: 2026-07-06

## Bewertung

| Bereich | Status | Nachweis |
| --- | --- | --- |
| Xcode Bootstrap | Done | `Docs/Platform/iOS_XcodeProjectBootstrap.md` |
| RKWP Client Flow | Done | `Docs/Platform/iOS_RKWPClientFlow.md` |
| Frame Presenter | Done | `Docs/Platform/iOS_FramePresenter.md` |
| Haptics | Planned | `Docs/Platform/iOS_HapticsAndGesturePrototype.md` |
| Gesture | Planned | Systemgesten muessen auf Hardware geprueft werden |
| No File Ingress | Done | `Docs/Platform/iOS_NoFileIngressSandboxChecklist.md` |
| USB Test | Done | `Docs/Platform/iOS_USBDeviceTestRunbook.md` |
| Return/Recovery | Done | `Docs/Platform/iOS_ReturnAndRecovery.md` |
| Share Extension Roadmap | Planned | `Docs/Platform/iOS_ShareExtensionObjectSource.md` |

## Windows-seitige Checks

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
.\tools\export-codex-context.ps1
```

## Blocker

- native Xcode-App fehlt in diesem Windows-Thread.
- echte iPhone/iPad-Hardwarepruefung fehlt.
- Local Network Prompt muss real bestaetigt werden.
- produktive Crypto fehlt.

## Ergebnis

iOS/iPadOS ist fuer den ersten nativen macOS-Codex/Xcode-Build vorbereitet. Der echte Owner-Test kann starten, sobald die App auf iPad/iPhone laeuft.
