# MA017 macOS Final Codex Handoff

Status: Draft
Datum: 2026-07-07

## Ziel

Dieser Handoff ist der finale Einstieg fuer eine macOS-Codex-/Xcode-Umsetzung nach MA017.

## Aufgabe fuer macOS

Baue eine native macOS Guest-Ablage, die RKWP-Kapseln und OpenFrames anzeigen kann.

Nicht bauen:

- freie PDF-Datei auf macOS
- Besitzuebergang
- Cloud-Sync
- produktive Discovery

## Pfade

- `release/ma017/handoff/macOS_START_HERE.md`
- `release/ma017/handoff/macOS_MinimalGuestAppTask.md`
- `release/ma017/runbooks/macOS_PilotRunbook.md`
- `release/ma017/packages/apple-rkwp-swift-bundle/`
- `release/ma017/schemas/macos-capsule-contract.v0.2.json`
- `release/ma017/schemas/macos-openframe-contract.v0.2.json`
- `release/ma017/schemas/macos-no-file-ingress-contract.v0.2.json`

## Definition of Done

```text
MacGuestIdentity: OK
FrameView: OK
Return: SUCCESS
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
