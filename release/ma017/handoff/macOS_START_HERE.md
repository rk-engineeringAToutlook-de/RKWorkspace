# macOS START HERE - MA017

Status: final handoff for macOS Codex/Xcode execution.
Datum: 2026-07-07

## Ziel

Baue die erste native macOS Guest Ablage, die Windows-owned PDF-Dinge als kontrollierte RKWP Frames sichtbar macht.

Die macOS Ablage darf standardmaessig niemals:

- die Original-PDF besitzen.
- die Original-PDF als freie Datei speichern.
- Originalbytes in Sandbox, Downloads, tmp oder Cache materialisieren.
- sichtbare Sprache fuer Transfer, Upload, Download, Sync, Server, Client, Device oder Agent verwenden.

## Reihenfolge

1. `release/ma017/packages/apple-rkwp-swift-bundle/README.md`
2. `release/ma017/packages/apple-rkwp-swift-bundle/RKWPModels.swift`
3. `release/ma017/config/macos-guest.sample.json`
4. `release/ma017/schemas/macos-capsule-contract.v0.2.json`
5. `release/ma017/schemas/macos-openframe-contract.v0.2.json`
6. `release/ma017/schemas/macos-no-file-ingress-contract.v0.2.json`
7. `release/ma017/handoff/macOS_MinimalGuestAppTask.md`
8. `release/ma017/runbooks/macOS_PilotRunbook.md`
9. `release/ma017/runbooks/macOS_BuildPermissionsChecklist.md`

## Erstes Ziel

Windows bleibt Owner von `Rechnung.pdf`.
macOS zeigt nur eine Kapsel oder einen OpenFrame.
macOS gibt zurueck oder wird vom Owner recovered.

## Smoke-Gate

Auf Windows:

~~~powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
~~~

Auf macOS/Xcode spaeter muss die native App dieselben Marker reproduzieren:

~~~text
MacGuestIdentity: OK
FrameView: OK
Return: SUCCESS
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
RESULT: SUCCESS
~~~
