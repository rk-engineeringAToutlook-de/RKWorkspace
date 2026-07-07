# iOS and iPadOS START HERE - MA017

Status: final handoff for Xcode execution through macOS.
Datum: 2026-07-07

## Ziel

iPhone und iPad werden mobile RK Workspace Ablagen.

Die erste App zeigt FrameCapsules und OpenFrames.
Sie darf die Original-PDF nicht als freie Datei speichern, exportieren oder ueber Files App / Share Sheet materialisieren.

## Reihenfolge

1. `release/ma017/packages/apple-rkwp-swift-bundle/README.md`
2. `release/ma017/packages/apple-rkwp-swift-bundle/RKWPModels.swift`
3. `release/ma017/config/ios-guest.sample.json`
4. `release/ma017/schemas/ios-capsule-contract.v0.2.json`
5. `release/ma017/schemas/ios-openframe-contract.v0.2.json`
6. `release/ma017/schemas/ios-haptics-contract.v0.2.json`
7. `release/ma017/schemas/ios-no-file-ingress-sandbox-contract.v0.2.json`
8. `release/ma017/handoff/iOS_XcodeMinimalAppTask.md`
9. `release/ma017/runbooks/iOS_USBInstallRunbook.md`
10. `release/ma017/runbooks/iOS_iPad_PilotRunbook.md`

## Sichtbare Sprache

Erlaubt:

- Ablage
- Ding
- Kapsel
- Frame
- liegt hier
- zurueckgeben
- wiederhergestellt
- nicht verfuegbar

Verboten:

- Transfer
- Upload
- Download
- Sync
- Device
- Agent
- Endpoint
- Server
- Client

## Smoke-Gate

Auf Windows:

~~~powershell
.\tools\run-ma017-ios-handoff.ps1 -SmokeTest
~~~

Auf iPad/iPhone spaeter:

~~~text
iOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
HapticsPrepared: OK
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
RESULT: SUCCESS
~~~
