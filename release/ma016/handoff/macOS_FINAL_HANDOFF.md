# MA016 macOS Final Handoff

## Goal

Build a native macOS Guest Surface that can receive a controlled FrameCapsule/OpenFrame from the Windows owner path without file ingress.

## Required inputs

- `release/ma016/handoff/macOS_START_HERE.md`
- `release/ma016/handoff/macOS_MinimalGuestAppTask.md`
- `release/ma016/schemas/macos-capsule-contract.v0.1.json`
- `release/ma016/schemas/macos-openframe-contract.v0.1.json`
- `release/ma016/schemas/macos-no-file-ingress-contract.v0.1.json`
- `release/ma016/packages/apple-rkwp-bundle/`

## Required proof

```text
MacGuestIdentity: OK
Platform: MacOS
FrameView: OK
NoFileIngress: SUCCESS
Return: SUCCESS
RESULT: SUCCESS
```

## Non-negotiable

The macOS surface must not persist the original PDF, original path or original bytes. It shows a controlled frame only.
