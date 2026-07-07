# MA017 macOS Handoff Package

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Paket ist die finale MA017-Uebergabe fuer macOS Codex/Xcode.

macOS wird als Guest-Ablage vorbereitet. Es darf keine Original-PDF als Datei besitzen.

## Reihenfolge

1. `release/ma017/handoff/macOS_START_HERE.md`
2. `release/ma017/handoff/macOS_FINAL_CODEX_HANDOFF.md`
3. `release/ma017/packages/apple-rkwp-swift-bundle/README.md`
4. `release/ma017/packages/apple-rkwp-swift-bundle/RKWPModels.swift`
5. `release/ma017/runbooks/macOS_PilotRunbook.md`
6. `release/ma017/runbooks/macOS_BuildPermissionsChecklist.md`

## Windows Gate

```powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
```

## Ergebnis

```text
macOSHandoffPackage: READY
MacGuestIdentity: OK
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
