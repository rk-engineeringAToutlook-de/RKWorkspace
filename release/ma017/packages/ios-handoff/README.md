# MA017 iOS/iPadOS Handoff Package

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Paket ist die finale MA017-Uebergabe fuer iOS/iPadOS Codex/Xcode.

iPad und iPhone werden als mobile Ablagen vorbereitet. Die App darf Original-PDFs nicht als freie Dateien materialisieren.

## Reihenfolge

1. `release/ma017/handoff/iOS_START_HERE.md`
2. `release/ma017/handoff/iOS_XCODE_FINAL_CODEX_HANDOFF.md`
3. `release/ma017/packages/apple-rkwp-swift-bundle/README.md`
4. `release/ma017/packages/apple-rkwp-swift-bundle/RKWPModels.swift`
5. `release/ma017/runbooks/iOS_USBInstallRunbook.md`
6. `release/ma017/runbooks/iOS_iPad_PilotRunbook.md`

## Windows Gate

```powershell
.\tools\run-ma017-ios-handoff.ps1 -SmokeTest
```

## Ergebnis

```text
iOSHandoffPackage: READY
iOSGuestAblage: STARTED
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
