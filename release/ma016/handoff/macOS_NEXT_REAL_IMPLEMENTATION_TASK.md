# macOS Next Real Implementation Task

## Goal

Build the first native macOS RK Workspace Guest Surface for MA016 follow-up.

## Inputs

- `release/ma016/handoff/macOS_FINAL_HANDOFF.md`
- `release/ma016/packages/apple-rkwp-bundle/`
- `release/ma016/schemas/macos-capsule-contract.v0.1.json`
- `release/ma016/schemas/macos-openframe-contract.v0.1.json`

## Required implementation

- Native Swift/SwiftUI macOS app.
- RKWP capsule/open-frame model parser.
- Frame-only display surface.
- Return command.
- No File Ingress audit log.
- Clear SecureDev warning for lab mode.

## Done when

```text
MacGuestIdentity: OK
FrameView: OK
Return: SUCCESS
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
