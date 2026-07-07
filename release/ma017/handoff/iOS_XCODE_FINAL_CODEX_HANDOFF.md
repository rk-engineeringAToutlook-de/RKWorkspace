# MA017 iOS/Xcode Final Codex Handoff

Status: Draft
Datum: 2026-07-07

## Ziel

Dieser Handoff ist der finale Einstieg fuer eine iOS/iPadOS-Codex-/Xcode-Umsetzung nach MA017.

## Aufgabe fuer iOS/iPadOS

Baue eine native iPad/iPhone Guest-Ablage, die RKWP-Kapseln und OpenFrames anzeigen kann.

Nicht bauen:

- freie PDF-Datei in Files App
- Originalbytes im App-Container
- Besitzuebergang
- produktive Discovery

## Pfade

- `release/ma017/handoff/iOS_START_HERE.md`
- `release/ma017/handoff/iOS_XcodeMinimalAppTask.md`
- `release/ma017/runbooks/iOS_USBInstallRunbook.md`
- `release/ma017/runbooks/iOS_iPad_PilotRunbook.md`
- `release/ma017/packages/apple-rkwp-swift-bundle/`
- `release/ma017/schemas/ios-capsule-contract.v0.2.json`
- `release/ma017/schemas/ios-openframe-contract.v0.2.json`
- `release/ma017/schemas/ios-haptics-contract.v0.2.json`
- `release/ma017/schemas/ios-no-file-ingress-sandbox-contract.v0.2.json`

## Definition of Done

```text
iOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
HapticsPrepared: OK
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
